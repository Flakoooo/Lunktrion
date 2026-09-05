using LunktrionApi.Services;
using LunktrionShared.Models.Enums;
using LunktrionShared.Models.Interfaces;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using LunktrionShared.Models.Utils;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace LunktrionApi.Hubs
{
    public class MainHub(
        DeviceService deviceService, 
        RabbitMqService rabbitMqService,
        ILogger<MainHub> logger
    ) : Hub, IHubContract
    {
        private readonly DeviceService _deviceService = deviceService;
        private readonly RabbitMqService _rabbitMqService = rabbitMqService;
        private readonly ILogger<MainHub> _logger = logger;

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var device = _deviceService.RemoveActiveDevice(Context.ConnectionId);
            if (device is not null)
            {
                await Clients.All.SendAsync(
                    HubCommands.DeviceOffline, device.DeviceId
                );

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Устройство {DeviceId} отключено",
                        device.DeviceId
                    );
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(
                        "Неизвестное устройство было отключено, Id соединения {ConnectionId}",
                        Context.ConnectionId
                    );
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task ExecuteErrorMessageAsync(string errorMessage, string errorLogMessage)
        {
            await Clients.Caller.SendAsync(
                HubCommands.Error,
                errorMessage
            );

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Ошибка: {Message}", errorLogMessage);
            }
        }

        private async Task<bool> CheckReceivedCommandAsync(string command, OperatingSystemType systemType)
        {
            if (systemType is OperatingSystemType.Windows)
            {
                if (WindowsValidator.IsSafe(command))
                {
                    return true;
                }

                await ExecuteErrorMessageAsync(
                    "Данная команда запрещена",
                    "Попытка вызова запрещенной команды на Windows"
                );

                return false;
            }

            return false;
        }

        // ОТПРАВКА СООБЩЕНИЙ КЛИЕНТОМ
                
        public async Task RegisterDevice(RegisterDeviceReuest request)
        {
            if (await _deviceService.Register(request, Context.ConnectionId))
            {
                await Clients.All.SendAsync(
                    HubCommands.DeviceOnline,
                    request.Identity
                );

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Устройство {DeviceId} подключено", request.Identity.DeviceUUID);
                }

                var cachedCommandResponse = await _deviceService.TryGetCachedDeviceExecuteCommandResponseAsync(request.Identity.DeviceUUID);
                if (cachedCommandResponse is not null)
                {
                    await Clients.Caller.SendAsync(
                        HubCommands.CommandResult, cachedCommandResponse
                    );

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Отправлен результат выполнения команды для устройства {DeviceId}", request.Identity.DeviceUUID);
                    }
                }

                await _rabbitMqService.GetMessageAsync(
                    RabbitMqService.GetDeviceCommandsKey(request.Identity.DeviceUUID),
                    async (routingKey, json) =>
                    {
                        DeviceExecuteCommandRequest? message;
                        try
                        {
                            message = JsonSerializer.Deserialize<DeviceExecuteCommandRequest>(json);
                        }
                        catch (JsonException ex)
                        {
                            if (_logger.IsEnabled(LogLevel.Error))
                            {
                                _logger.LogError(ex, "Критическая ошибка: в поток {Key} прилетел некорректный JSON", routingKey);
                            }

                            return;
                        }

                        await Clients.Caller.SendAsync(
                            HubCommands.ExecuteCommand, message
                        );
                    },
                    QueueReadStrategy.DrainExistingOnly,
                    logStart: $"Проверка отложенных команд для устройства {request.Identity.DeviceUUID}...",
                    logSuccess: $"Отложенная команда успешно доставлена на устройство {request.Identity.DeviceUUID}",
                    logFail: $"Соединение разорвано при доставке отложенной команды для {request.Identity.DeviceUUID}. Команда возвращена в очередь."
                );

                return;
            }

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Устройство {DeviceId} не удалось подключить", request.Identity.DeviceUUID);
            }
        }
                
        /// <summary>
        /// Метод для запроса обновления краткой информации о устройстве
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task RequestDeviceInfo(DeviceInfoRequest request)
        {
            var targetDevice = _deviceService.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                string error = $"Устройство {request.TargetDeviceId} не в сети";
                await ExecuteErrorMessageAsync(error, error);
                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                HubCommands.CollectAndSendInfo, request
            );

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Запрошена информация о устройстве {TargetDeviceId}", request.TargetDeviceId);
        }

        public async Task RequestDeviceCommand(DeviceExecuteCommandRequest request)
        {
            var targetDevice = _deviceService.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                var device = await _deviceService.GetDeviceByDeviceId(request.TargetDeviceId);
                if (device is null)
                {
                    await ExecuteErrorMessageAsync(
                        "Неизвестное устройство",
                        $"Устройство с ID {request.TargetDeviceId} неизвестно. Отмена выполнения отправленной команды."
                    );

                    return;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Устройство {TargetDeviceId} офлайн. Сохраняем команду в RabbitMQ.",
                        request.TargetDeviceId
                    );
                }

                if (!await CheckReceivedCommandAsync(request.Command, device.OperatingSystemType))
                {
                    return;
                }

                await _rabbitMqService.SendMessageAsync(request.TargetDeviceId, request);

                await Clients.Caller.SendAsync(
                    HubCommands.Notification,
                    $"Устройство {request.TargetDeviceId} сейчас не в сети. Команда поставлена в очередь и выполнится при его включении."
                );

                return;
            }

            if (!await CheckReceivedCommandAsync(request.Command, targetDevice.OperatingSystemType))
            {
                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                HubCommands.ExecuteCommand, request
            );

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Запрошен вызов команды на устройстве {TargetDeviceId}", request.TargetDeviceId);
            }
        }

        // ПОЛУЧЕНИЕ ОТВЕТА ОТ КЛИЕНТА

        public async Task ReceiveDeviceInfo(DeviceInfoResponse response)
        {
            await _deviceService.UpdateDeviceInfo(response);

            // нужно удалить из кэша возможные старые данные

            var targetDevice = _deviceService.GetActiveDeviceByDeviceId(response.RequestorDeviceId);
            if (targetDevice is null)
            {
                string error = $"Устройство {response.RequestorDeviceId} не в сети";
                await ExecuteErrorMessageAsync(error, error);

                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                HubCommands.DeviceInfoReceived, response
            );

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Информация о устройстве {TargetDeviceId} была отправлена устройству {RequestorDeviceId}",
                    response.TargetDeviceId, response.RequestorDeviceId
                );
            }
        }
                
        public async Task ReceiveCommandResult(DeviceExecuteCommandResponse response)
        {
            var targetDevice = _deviceService.GetActiveDeviceByDeviceId(response.RequestorDeviceId);
            if (targetDevice is null)
            {
                string error = $"Устройство {response.RequestorDeviceId} не в сети";
                await ExecuteErrorMessageAsync(error, error);

                await _deviceService.SetDeviceExecuteCommandResponseInCacheAsync(response);

                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                HubCommands.CommandResult, response
            );
        }
    }
}
