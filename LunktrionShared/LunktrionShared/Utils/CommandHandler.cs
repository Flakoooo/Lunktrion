using LunktrionShared.Models.Enums;

namespace LunktrionShared.Utils
{
    public class CommandHandler
    {
        public static string ShutdownCommand(
            OperatingSystemType systemType, ushort time = 300
        ) => systemType switch
        {
            OperatingSystemType.Windows => $"shutdown /s /f /t {time}",
            _ => string.Empty
        };
    }
}
