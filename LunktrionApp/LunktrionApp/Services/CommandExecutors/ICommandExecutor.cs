using System.Threading.Tasks;

namespace LunktrionApp.Services.CommandExecutors
{
    public interface ICommandExecutor
    {
        Task<CommandExecutorResult> ExecuteCommandAsync(string command);
    }
}
