using LunktrionApi.Models.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace LunktrionApi.Services
{
    public enum QueueReadStrategy
    {
        /// <summary>
        /// Постоянная подписка (Push).
        /// </summary>
        ContinuousSubscribe,

        /// <summary>
        /// Разовая вычитка до пустой очереди (Pull).
        /// </summary>
        DrainExistingOnly
    }

    public class RabbitMqService : IAsyncDisposable, IAsyncInitializer
    {
        private readonly ConnectionFactory _factory;
        private readonly ILogger<RabbitMqService> _logger;
        private IConnection? _connection;

        /// <summary>
        /// Возвращает строку форматом "device.commands.{deviceId}"
        /// </summary>
        /// <param name="deviceId">Id устройства</param>
        /// <returns></returns>
        public static string GetDeviceCommandsKey(string deviceId) => $"device.commands.{deviceId}";

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var connection = await GetConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                string exchangeName = "lunktrion.device.exchange";
                await channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("[RabbitMQ] Базовая топология успешно создана и готова к работе.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[RabbitMQ] Не удалось инициализировать топологию очередей при старте!");
                throw;
            }
        }

        public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
        {
            var connectionString = configuration.GetConnectionString("RabbitMQ");
            _factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString!),
                ConsumerDispatchConcurrency = 2
            };

            _logger = logger;
        }

        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection is null || !_connection.IsOpen)
                _connection = await _factory.CreateConnectionAsync(cancellationToken);

            return _connection;
        }

        private async Task ReadMessage(
            IChannel channel, 
            byte[] byteMessage, 
            ulong deliveryTag,
            string routingKey,
            Func<string, string, Task> handler,
            bool requeueOnError,
            string logPrefix,
            string? logSuccess = null,
            string? logFail = null,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var json = Encoding.UTF8.GetString(byteMessage);

                await handler.Invoke(routingKey, json);

                await channel.BasicAckAsync(
                    deliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken
                );

                if (!string.IsNullOrWhiteSpace(logSuccess) && _logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("{Prefix} Успех: {Log}", logPrefix, logSuccess);
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(logFail) && _logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex, "{Prefix} Ошибка обработки сообщения: {Log}", logPrefix, logFail);
                }

                await channel.BasicNackAsync(
                    deliveryTag, 
                    multiple: false, 
                    requeue: requeueOnError, 
                    cancellationToken: cancellationToken
                );

                throw;
            }
        }

        /// <summary>
        /// Универсальный метод для получения сообщений из брокера
        /// </summary>
        /// <param name="queueKey"></param>
        /// <param name="handler">Передает в функцию первым параметром routingKey, вторым json</param>
        /// <param name="strategy"></param>
        /// <param name="requeueOnError"></param>
        /// <param name="customLogPrefix"></param>
        /// <param name="logStart"></param>
        /// <param name="logSuccess"></param>
        /// <param name="logFail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetMessageAsync(
            string queueKey, 
            Func<string, string, Task> handler,
            QueueReadStrategy strategy,
            bool requeueOnError = false,
            string? customLogPrefix = null,
            string? logStart = null,
            string? logSuccess = null, 
            string? logFail = null,
            CancellationToken cancellationToken = default
        )
        {
            var logPrefix = customLogPrefix ?? $"[RabbitMQ:{queueKey}]";

            try
            {
                var connection = await GetConnectionAsync(cancellationToken);
                var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await channel.QueueDeclareAsync(
                    queue: queueKey, 
                    durable: true, 
                    exclusive: false, 
                    autoDelete: false, 
                    cancellationToken: cancellationToken
                );

                await channel.BasicQosAsync(
                    prefetchSize: 0, 
                    prefetchCount: 1, 
                    global: false, 
                    cancellationToken: cancellationToken
                );

                if (!string.IsNullOrWhiteSpace(logStart) && _logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("{Prefix} Старт по стратегии {Strategy}: {Log}", logPrefix, strategy, logStart);
                }

                if (strategy is QueueReadStrategy.ContinuousSubscribe)
                {
                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        try
                        {
                            await ReadMessage(
                                channel, 
                                ea.Body.ToArray(), 
                                ea.DeliveryTag,
                                ea.RoutingKey,
                                handler, 
                                requeueOnError, 
                                logPrefix, 
                                logSuccess, 
                                logFail, 
                                cancellationToken
                            );
                        }
                        catch { }
                    };

                    await channel.BasicConsumeAsync(
                        queue: queueKey, 
                        autoAck: false, 
                        consumer: consumer, 
                        cancellationToken: cancellationToken
                    );

                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                else if (strategy is QueueReadStrategy.DrainExistingOnly)
                {
                    await using (channel)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await channel.BasicGetAsync(queue: queueKey, autoAck: false, cancellationToken: cancellationToken);

                            if (result is null)
                            {
                                if (_logger.IsEnabled(LogLevel.Information))
                                {
                                    _logger.LogInformation("{Prefix} Все накопленные сообщения успешно обработаны.", logPrefix);
                                }

                                break;
                            }

                            try
                            {
                                await ReadMessage(
                                    channel, 
                                    result.Body.ToArray(), 
                                    result.DeliveryTag,
                                    result.RoutingKey,
                                    handler, 
                                    requeueOnError, 
                                    logPrefix, 
                                    logSuccess, 
                                    logFail, 
                                    cancellationToken
                                );
                            }
                            catch
                            {
                                if (!requeueOnError) break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex, "{Prefix} Сбой инициализации работы с очередью.", logPrefix);
                }
            }
        }

        public async Task SendMessageAsync<T>(string queueKey, T message)
        {
            var connection = await GetConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queueKey,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueKey,
                mandatory: true,
                basicProperties: properties,
                body: body
            );
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}
