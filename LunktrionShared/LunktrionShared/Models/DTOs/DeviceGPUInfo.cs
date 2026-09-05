namespace LunktrionShared.Models.DTOs
{
    public record class DeviceGPUInfo(
        string Name = "ОШИБКА",
        ulong VideoRAM = 0
    )
    {
        public string SpecificationText => $"Объем {VideoRAM / 1024.0 / 1024.0} MB";
    }
}
