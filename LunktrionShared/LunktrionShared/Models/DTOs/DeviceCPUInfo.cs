namespace LunktrionShared.Models.DTOs
{
    public record class DeviceCPUInfo(
        string Name = "ОШИБКА",
        uint NumberOfCores = 0,
        uint NumberOfLogicalProcessors = 0,
        uint CurrentClockSpeed = 0,
        uint MaxClockSpeed = 0
    );
}
