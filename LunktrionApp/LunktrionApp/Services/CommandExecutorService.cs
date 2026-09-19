using LunktrionApp.Hubs;
using LunktrionApp.Services.CommandExecutors;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using System;

namespace LunktrionApp.Services
{
    public class CommandExecutorService : IDisposable
    {
        private readonly DeviceIdentityService _deviceIdentityService;
        private readonly MainHub _mainHub;
        private readonly NotificationService _notificationService;
        private readonly ICommandExecutor _commandExecutor;

        public CommandExecutorService(
            DeviceIdentityService deviceIdentityService,
            MainHub mainHub,
            NotificationService notificationService,
            ICommandExecutor commandExecutor
        )
        {
            _deviceIdentityService = deviceIdentityService;
            _mainHub = mainHub;
            _notificationService = notificationService;
            _commandExecutor = commandExecutor;

            _mainHub.CommandReceived += OnCommandReceived;
        }

        private async void OnCommandReceived(DeviceExecuteCommandRequest request)
        {
            var device = await _deviceIdentityService.GetCurrentDeviceAsync();

            //if (string.Equals(device.DeviceId, request.TargetDeviceId, StringComparison.Ordinal)) 
            //    return;

            CommandExecutorResult result;

            try
            {
                result = await _commandExecutor.ExecuteCommandAsync(request.Command);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Вызванная команда не выполнилась: {ex.Message}");
                result = new CommandExecutorResult(false, $"Критическая ошибка: {ex.Message}");
            }

            await _mainHub.SendCommandResultAsync(
                new DeviceExecuteCommandResponse(
                    request.Command,
                    result.Output,
                    request.TargetDeviceId,
                    request.RequestorDeviceId,
                    request.RequestedAt,
                    DateTime.Now
                )
            );
        }

        public void Dispose()
        {
            _mainHub.CommandReceived -= OnCommandReceived;
        }
    }
}
