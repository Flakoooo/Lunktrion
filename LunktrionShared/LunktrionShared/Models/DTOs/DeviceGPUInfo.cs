namespace LunktrionShared.Models.DTOs
{
    public record class DeviceGPUInfo(
        string Name = "ОШИБКА",
        ulong VideoRAM = 0,
        uint MaxRefreshRate = 0
    );
}
