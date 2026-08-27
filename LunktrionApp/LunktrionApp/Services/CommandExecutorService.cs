using LunktrionApp.Hubs;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LunktrionApp.Services
{
    public class CommandExecutorService : IDisposable
    {
        private readonly DeviceIdentityService _deviceIdentityService;
        private readonly MainHub _mainHub;

        public CommandExecutorService(
            DeviceIdentityService deviceIdentityService,
            MainHub mainHub
        )
        {
            _deviceIdentityService = deviceIdentityService;
            _mainHub = mainHub;

            _mainHub.CommandReceived += OnCommandReceived;
        }

        public async Task<string> ExecuteWinCommandAsync(string command)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var ibm866 = Encoding.GetEncoding(866);

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = ibm866,
                    StandardErrorEncoding = ibm866
                };

                using var process = new Process { StartInfo = startInfo };

                if (!process.Start())
                {
                    return "Ошибка: Не удалось запустить процесс cmd.exe";
                }

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

                string output = await outputTask;
                string error = await errorTask;

                if (!string.IsNullOrWhiteSpace(error))
                    return $"Ошибка выполнения:\n{error}\n\nВывод системы:\n{output}";

                return output;
            }
            catch (Exception ex)
            {
                return $"Исключение при вызове: {ex.Message}";
            }
        }

        private async void OnCommandReceived(DeviceExecuteCommandRequest request)
        {
            var device = await _deviceIdentityService.GetCurrentDeviceAsync();

            //if (string.Equals(device.DeviceId, request.TargetDeviceId, StringComparison.Ordinal)) 
            //    return;

            string result;

            try
            {
                result = await ExecuteWinCommandAsync(request.Command);
            }
            catch (Exception ex)
            {
                //TODOO: Сделать вывод ошибки?
                result = $"Критическая ошибка: {ex.Message}";
                Debug.WriteLine(result);
            }

            await _mainHub.SendCommandResultAsync(
                new DeviceExecuteCommandResponse(
                    request.Command,
                    result,
                    request.TargetDeviceId,
                    request.RequestorDeviceId
                )
            );
        }

        public void Dispose()
        {
            _mainHub.CommandReceived -= OnCommandReceived;
        }
    }
}
