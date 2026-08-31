using LunktrionApi.Models.Entities;
using LunktrionApi.Services;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Interfaces;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using LunktrionShared.Models.Utils;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;

namespace LunktrionApi.Hubs
{
    public class MainHub(
        DeviceRegistry deviceRegistry, 
        RabbitMqService rabbitMqService,
        ILogger<MainHub> logger
    ) : Hub, IHubContract
    {
        private readonly DeviceRegistry _deviceRegistry = deviceRegistry;
        private readonly RabbitMqService _rabbitMqService = rabbitMqService;
        private readonly ILogger<MainHub> _logger = logger;

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var device = _deviceRegistry.Remove(Context.ConnectionId);
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

        private async Task<bool> CheckReceivedCommandAsync(string command, DeviceIdentity device)
        {
            if (device.OperatingSystemName.Contains("Windows", StringComparison.Ordinal))
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

        // ОТПРАВКА СООБЩЕНИЙ КЛИЕНТАМ
                
        public async Task RegisterDevice(DeviceIdentity device)
        {
            if (_deviceRegistry.Register(device, Context.ConnectionId))
            {
                await Clients.All.SendAsync(
                    HubCommands.DeviceOnline, 
                    new DeviceIdentity(
                        device.DeviceId, 
                        device.DeviceName, 
                        device.DeviceManufacturer, 
                        device.OperatingSystemName
                    )
                );

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Устройство {DeviceId} подключено", device.DeviceId);
                }

                var cachedCommandResponse = await _deviceRegistry.TryGetCachedDeviceExecuteCommandResponseAsync(device.DeviceId);
                if (cachedCommandResponse is not null)
                {
                    await Clients.Caller.SendAsync(
                        HubCommands.CommandResult, cachedCommandResponse
                    );

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Отправлен результат выполнения команды для устройства {DeviceId}", device.DeviceId);
                    }
                }

                try
                {
                    var connection = await _rabbitMqService.GetConnectionAsync();
                    using var channel = await connection.CreateChannelAsync();

                    var queueName = $"device.commands.{device.DeviceId}";

                    await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Проверка отложенных команд для устройства {DeviceId}...", device.DeviceId);
                    }

                    while (true)
                    {
                        var result = await channel.BasicGetAsync(queue: queueName, autoAck: false);

                        if (result is null) 
                            break;

                        var json = Encoding.UTF8.GetString(result.Body.ToArray());
                        var archivedRequest = JsonSerializer.Deserialize<DeviceExecuteCommandRequest>(json);

                        if (archivedRequest is not null)
                        {
                            try
                            {
                                await Clients.Caller.SendAsync(
                                    HubCommands.ExecuteCommand, archivedRequest
                                );

                                await channel.BasicAckAsync(deliveryTag: result.DeliveryTag, multiple: false);

                                if (_logger.IsEnabled(LogLevel.Information))
                                {
                                    _logger.LogInformation("Отложенная команда успешно доставлена на устройство {DeviceId}", device.DeviceId);
                                }
                            }
                            catch (Exception ex)
                            {
                                await channel.BasicNackAsync(deliveryTag: result.DeliveryTag, multiple: false, requeue: true);
                                
                                if (_logger.IsEnabled(LogLevel.Error))
                                {
                                    _logger.LogError(ex, "Соединение разорвано при доставке отложенной команды для {DeviceId}. Команда возвращена в очередь.", device.DeviceId);
                                }

                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке очереди RabbitMQ для устройства {DeviceId}", device.DeviceId);
                }

                return;
            }

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Устройство {DeviceId} не удалось подключить", device.DeviceId);
            }
        }
                
        public async Task RequestDeviceInfo(DeviceInfoRequest request)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                string error = $"Устройство {request.TargetDeviceId} не в сети";
                await ExecuteErrorMessageAsync(error, error);
                return;
            }

            var cachedInfo = await _deviceRegistry.TryGetCachedDeviceInfoAsync(request.TargetDeviceId);
            if (cachedInfo is not null)
            {
                await Clients.Caller.SendAsync(
                    HubCommands.DeviceInfoReceived, 
                    new DeviceInfoResponse(cachedInfo, request.TargetDeviceId, request.RequestorDeviceId)
                );

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Запрошеная информация о устройстве {TargetDeviceId} получена из кэша", request.TargetDeviceId);
                }

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
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                var device = _deviceRegistry.GetDeviceByDeviceId(request.TargetDeviceId);
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

                if (!await CheckReceivedCommandAsync(request.Command, device))
                {
                    return;
                }

                await _rabbitMqService.SendCommandAsync(request.TargetDeviceId, request);

                await Clients.Caller.SendAsync(
                    HubCommands.Notification,
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

        public async Task RequestBrowseDirectory(string targetDeviceId, string path)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(targetDeviceId);
            if (targetDevice is null)
            {
                string error = $"Устройство {targetDeviceId} не в сети";
                await ExecuteErrorMessageAsync(error, error);
                return;
            }

            await Clients.Client(targetDevice.ConnectionId)
                .SendAsync("BrowseDirectory", path, Context.ConnectionId);
        }

        // ПОЛУЧЕНИЕ ОТВЕТА ОТ КЛИЕНТА

        public async Task ReceiveDeviceInfo(DeviceInfoResponse response)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(response.RequestorDeviceId);
            if (targetDevice is null)
            {
                string error = $"Устройство {response.RequestorDeviceId} не в сети";
                await ExecuteErrorMessageAsync(error, error);

                return;
            }

            await _deviceRegistry.SetDeviceInfoInCacheAsync(response);

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
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(response.RequestorDeviceId);
            if (targetDevice is null)
            {
                string error = $"Устройство {response.RequestorDeviceId} не в сети";
                await ExecuteErrorMessageAsync(error, error);

                await _deviceRegistry.SetDeviceExecuteCommandResponseInCacheAsync(response);

                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                HubCommands.CommandResult, response
            );
        }

        public Task ReceiveBrowseResult(List<FileSystemEntry> entries, string requestorConnectionId)
        {
            return Clients.Client(requestorConnectionId)
                .SendAsync("BrowseResult", entries);
        }
    }
}
