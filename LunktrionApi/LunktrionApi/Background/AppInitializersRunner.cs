using LunktrionApi.Models.Interfaces;

namespace LunktrionApi.Background
{
    public class AppInitializersRunner(
        IEnumerable<IAsyncInitializer> initializers,
        ILogger<AppInitializersRunner> logger
    ) : IHostedLifecycleService
    {
        private readonly IEnumerable<IAsyncInitializer> _initializers = initializers;
        private readonly ILogger<AppInitializersRunner> _logger = logger;

        public async Task StartingAsync(CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("[Инициализация] Настройка инфраструктуры...");
            }

            foreach (var initializer in _initializers)
            {
                await initializer.InitializeAsync(cancellationToken);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("[Инициализация] Все системы готовы. Запуск веб-сервера");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
