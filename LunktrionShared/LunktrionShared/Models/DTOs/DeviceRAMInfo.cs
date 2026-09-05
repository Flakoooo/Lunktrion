namespace LunktrionShared.Models.DTOs
{
    public record class DeviceRAMInfo(
        string Manufacturer = "ОШИБКА",
        ulong Size = 0,
        string Type = "ОШИБКА",
        uint Speed = 0
    )
    {
        public string SpecificationText => $"Тип {Type}, Частота {Speed} MHz, Объём {Size / 1024.0 / 1024.0 / 1024.0} GB";
    }
}
