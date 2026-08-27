using LunktrionApi.Models.Entities;
using LunktrionApi.Services;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;

namespace LunktrionApi.Hubs
{
    public class MainHub(
        DeviceRegistry deviceRegistry, 
        RabbitMqService rabbitMqService,
        ILogger<MainHub> logger
    ) : Hub
    {
        private readonly DeviceRegistry _deviceRegistry = deviceRegistry;
        private readonly RabbitMqService _rabbitMqService = rabbitMqService;
        private readonly ILogger<MainHub> _logger = logger;

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var device = _deviceRegistry.Remove(Context.ConnectionId);
            if (device is not null)
            {
                await Clients.All.SendAsync("DeviceOffline", device.DeviceId);

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

        private async Task ExecuteErrorMessage(string targetDeviceId)
        {
            await Clients.Caller.SendAsync("Error", $"Устройство {targetDeviceId} не в сети");

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Устройство {TargetDeviceId} не в сети", targetDeviceId);
            }
        }

        // ОТПРАВКА СООБЩЕНИЙ КЛИЕНТАМ

        /// <summary>
        /// Регистрационный метод для устройств. Необходимо его вызвать для уведомления всех о новом устрйостве в системе
        /// </summary>
        /// <param name="device">Регистрируемый девайс, который подключается к системе</param>
        /// <returns></returns>
        public async Task RegisterDevice(DeviceIdentity device)
        {
            if (_deviceRegistry.Register(device, Context.ConnectionId))
            {
                await Clients.All.SendAsync(
                    "DeviceOnline", 
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
                                await Clients.Caller.SendAsync("ExecuteCommand", archivedRequest);

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

        /// <summary>
        /// Метод для запроса краткой информации о устройстве (CPU, GPU, RAM и память)
        /// </summary>
        /// <param name="targetDeviceId">Id устройства, у которого запрашивается информация</param>
        /// <returns></returns>
        public async Task RequestDeviceInfo(DeviceInfoRequest request)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                await ExecuteErrorMessage(request.TargetDeviceId);
                return;
            }

            var cachedInfo = await _deviceRegistry.TryGetCachedDeviceInfoAsync(request.TargetDeviceId);
            if (cachedInfo is not null)
            {
                await Clients.Caller.SendAsync(
                    "DeviceInfoReceived", 
                    new DeviceInfoResponse(cachedInfo, request.TargetDeviceId, request.RequestorDeviceId)
                );

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Запрошеная информация о устройстве {TargetDeviceId} получена из кэша", request.TargetDeviceId);

                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                "CollectAndSendInfo",
                request
            );

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Запрошена информация о устройстве {TargetDeviceId}", request.TargetDeviceId);
        }

        /// <summary>
        /// Метод для выполнения команды на требуемом устройстве
        /// </summary>
        /// <param name="targetDeviceId">Id устройства, у которого нужно выполнить команду</param>
        /// <param name="command">Команда, которая должна выполнить на устройстве</param>
        /// <returns></returns>
        public async Task RequestDeviceCommand(DeviceExecuteCommandRequest request)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(request.TargetDeviceId);
            if (targetDevice is null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Устройство {TargetDeviceId} офлайн. Сохраняем команду в RabbitMQ.", 
                        request.TargetDeviceId
                    );
                }

                await _rabbitMqService.SendCommandAsync(request.TargetDeviceId, request);

                await Clients.Caller.SendAsync(
                    "Notification", 
                    $"Устройство {request.TargetDeviceId} сейчас не в сети. Команда поставлена в очередь и выполнится при его включении."
                );

                return;
            }

            await Clients.Client(targetDevice.ConnectionId).SendAsync(
                "ExecuteCommand",
                request
            );

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Запрошен вызов команды на устройстве {TargetDeviceId}", request.TargetDeviceId);
        }

        public async Task RequestBrowseDirectory(string targetDeviceId, string path)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(targetDeviceId);
            if (targetDevice is null)
            {
                await ExecuteErrorMessage(targetDeviceId);
                return;
            }

            await Clients.Client(targetDevice.ConnectionId)
                .SendAsync("BrowseDirectory", path, Context.ConnectionId);
        }

        // ПОЛУЧЕНИЕ ОТВЕТА ОТ КЛИЕНТА

        /// <summary>
        /// Метод возвращающий краткую информацию о устройстве
        /// </summary>
        /// <param name="response">Информация, которую собрало устройство</param>
        /// <returns></returns>
        public async Task ReceiveDeviceInfo(DeviceInfoResponse response)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(response.RequestorDeviceId);
            if (targetDevice is null)
            {
                await ExecuteErrorMessage(response.RequestorDeviceId);
                return;
            }

            await _deviceRegistry.SetDeviceInfoInCacheAsync(response.TargetDeviceId, response.Info);

            await Clients.Client(targetDevice.ConnectionId)
                .SendAsync("DeviceInfoReceived", response);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Информация о устройстве {TargetDeviceId} была отправлена устройству {RequestorDeviceId}",
                    response.TargetDeviceId, response.RequestorDeviceId
                );
            }
        }

        /// <summary>
        /// Метод возвращающий результат выполненной команды на устройстве
        /// </summary>
        /// <param name="response">Результат выполненой команды на устройстве</param>
        /// <returns></returns>
        public async Task ReceiveCommandResult(DeviceExecuteCommandResponse response)
        {
            var targetDevice = _deviceRegistry.GetActiveDeviceByDeviceId(response.RequestorDeviceId);
            if (targetDevice is null)
            {
                await ExecuteErrorMessage(response.RequestorDeviceId);
                return;
            }

            await Clients.Client(targetDevice.ConnectionId)
                .SendAsync("CommandResult", response);
        }

        public Task ReceiveBrowseResult(List<FileSystemEntry> entries, string requestorConnectionId)
        {
            return Clients.Client(requestorConnectionId)
                .SendAsync("BrowseResult", entries);
        }
    }
}
