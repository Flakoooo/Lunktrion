using LunktrionApi.Services;
using LunktrionShared.Models.Enums;
using LunktrionShared.Models.Interfaces;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using LunktrionShared.Models.Utils;
using LunktrionShared.Utils;
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

        private enum ErrorType
        {
            Unknown,
            UnknownOperatingSystem,
            Offline,
            ForbiddenCommand,
            ShutdownWaiting
        }

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

        private async Task ExecuteErrorMessageAsync(
            ErrorType errorType, string deviceId, 
            string? errorMessage = null, string? errorLogMessage = null
        )
        {
            if (errorType is ErrorType.Unknown)
            {
                errorMessage ??= "Неизвестное устройство";
                errorLogMessage ??= $"Устройство с ID {deviceId} неизвестно";
            }
            else if (errorType is ErrorType.UnknownOperatingSystem)
            {
                errorMessage ??= "Неизвестная операционна система. Обновить данные об устройстве";
                errorLogMessage ??= $"Устройство с ID {deviceId} имеет неизвестную операционную систему";
            }
            else if (errorType is ErrorType.Offline)
            {
                errorMessage ??= $"Устройство {deviceId} не в сети";
                errorLogMessage ??= $"Устройство {deviceId} не в сети";
            }
            else if (errorType is ErrorType.ForbiddenCommand)
            {
                errorMessage ??= "Данная команда запрещена";
                errorLogMessage ??= "Попытка вызова запрещенной команды";
            }
            else if (errorType is ErrorType.ShutdownWaiting)
            {
                errorMessage ??= "Запрос на выключение уже отправлен";
                errorLogMessage ??= $"Попытка повторной отправки запрос на выключение на устройство {deviceId}";
            }
            else
            {
                errorMessage ??= "Непредвиденная ошибка";
                errorLogMessage ??= "Непредвиденная ошибка";
            }

            await Clients.Caller.SendAsync(
                HubCommands.Error,
                errorMessage
            );

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Ошибка: {Message}", errorLogMessage);
            }
        }

        private async Task ExecuteNotificationMessageAsync(string notificationMessage) => await Clients.Caller.SendAsync(
            HubCommands.Notification,
            notificationMessage
        );

        private async Task<bool> CheckReceivedCommandAsync(string command, IDevice device)
        {
            if (device.OperatingSystemType is OperatingSystemType.Windows)
            {
                if (WindowsValidator.IsSafe(command))
                {
                    return true;
                }

                await ExecuteErrorMessageAsync(
                    ErrorType.ForbiddenCommand, device.DeviceUUID,
                    errorLogMessage: "Попытка вызова запрещенной команды на Windows"
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
        public async Task RequestUpdateDeviceInfo(DeviceInfoRequest request)
        {
            var targetDevice = _deviceService.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                await ExecuteErrorMessageAsync(ErrorType.Offline, request.TargetDeviceId);
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
                        ErrorType.Unknown, request.TargetDeviceId,
                        errorLogMessage: $"Устройство с ID {request.TargetDeviceId} неизвестно. Отмена выполнения отправленной команды."
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

                if (!await CheckReceivedCommandAsync(request.Command, device))
                {
                    return;
                }

                await _rabbitMqService.SendMessageAsync(request.TargetDeviceId, request);

                await ExecuteNotificationMessageAsync(
                    $"Устройство {request.TargetDeviceId} сейчас не в сети. Команда поставлена в очередь и выполнится при его включении."
                );

                return;
            }

            if (!await CheckReceivedCommandAsync(request.Command, targetDevice))
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
                
        public async Task RequestDeviceShutdown(DeviceShutdownRequest request)
        {
            var targetDevice = _deviceService.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                await ExecuteErrorMessageAsync(ErrorType.Offline, request.TargetDeviceId);
                return;
            }

            if (targetDevice.WaitingForShutdown)
            {
                await ExecuteErrorMessageAsync(
                    ErrorType.ShutdownWaiting, request.TargetDeviceId
                );

                return;
            }

            targetDevice.WaitingForShutdown = true;

            bool isPinCodeCorrected = _deviceService.VerifyCode(request.Code);

            string shutdownCommand = CommandHandler.ShutdownCommand(
                targetDevice.OperatingSystemType, 
                (ushort)(isPinCodeCorrected ? 0 : 300)
            );

            if (string.IsNullOrWhiteSpace(shutdownCommand))
            {
                await ExecuteErrorMessageAsync(
                    ErrorType.UnknownOperatingSystem, request.TargetDeviceId
                );
                return;
            }

            var shutdownAt = DateTime.Now;
            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                HubCommands.ExecuteCommand,
                new DeviceExecuteCommandRequest(
                    request.TargetDeviceId, request.RequestorDeviceId, shutdownCommand
                )
            );

            await ExecuteNotificationMessageAsync(
                $"Запрос на выключение устройства отправлен. Устройство будет выключено {(isPinCodeCorrected ? "в ближайщее время" : "через 5 минут")}"
            );

            if (!isPinCodeCorrected)
            {
                await Clients.All.SendAsync(
                    HubCommands.ShutdownCommandNotification,
                    new DeviceShutdownResponse(
                        request.TargetDeviceId, request.RequestorDeviceId, shutdownAt
                    )
                );
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Запрошен вызов отключения устройства {TargetDeviceId}", request.TargetDeviceId);
            }
        }

        // ПОЛУЧЕНИЕ ОТВЕТА ОТ КЛИЕНТА

        public async Task ReceiveNewDeviceInfo(DeviceInfoResponse response)
        {
            await _deviceService.UpdateDeviceInfo(response);

            var targetDevice = _deviceService.GetActiveDeviceByDeviceId(response.RequestorDeviceId);
            if (targetDevice is null)
            {
                await ExecuteErrorMessageAsync(ErrorType.Offline, response.RequestorDeviceId);
                return;
            }

            await Clients.All.SendAsync(
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
                await ExecuteErrorMessageAsync(ErrorType.Offline, response.RequestorDeviceId);

                await _deviceService.SetDeviceExecuteCommandResponseInCacheAsync(response);

                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                HubCommands.CommandResult, response
            );
        }
    }
}
