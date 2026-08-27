using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LunktrionApi.Services
{
    public class RabbitMqService : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;

        public RabbitMqService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("RabbitMQ");
            _factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString!),
                ConsumerDispatchConcurrency = 2
            };
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            if (_connection is null || !_connection.IsOpen)
                _connection = await _factory.CreateConnectionAsync();

            return _connection;
        }

        public async Task SendCommandAsync<T>(string deviceId, T command)
        {
            var connection = await GetConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var queueName = $"device.commands.{deviceId}";

            await channel.QueueDeclareAsync(
                queue: queueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null
            );

            var json = JsonSerializer.Serialize(command);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

            await channel.BasicPublishAsync(
                exchange:  string.Empty, 
                routingKey: queueName, 
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
