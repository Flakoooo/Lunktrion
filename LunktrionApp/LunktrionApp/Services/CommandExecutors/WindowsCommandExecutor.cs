using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LunktrionApp.Services.CommandExecutors
{
    public class WindowsCommandExecutor : ICommandExecutor
    {
        public async Task<CommandExecutorResult> ExecuteCommandAsync(string command)
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
                    return new CommandExecutorResult(false, "Ошибка: Не удалось запустить процесс cmd.exe");
                }

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

                string output = await outputTask;
                string error = await errorTask;

                if (!string.IsNullOrWhiteSpace(error))
                {
                    return new CommandExecutorResult(false, $"Ошибка выполнения:\n{error}\n\nВывод системы:\n{output}");
                }

                return new CommandExecutorResult(true, output);
            }
            catch (Exception ex)
            {
                return new CommandExecutorResult(false, $"Исключение при вызове: {ex.Message}");
            }
        }
    }
}
