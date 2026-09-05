namespace LunktrionShared.Models.DTOs
{
    public record class DeviceCPUInfo(
        string Name = "ОШИБКА",
        short NumberOfCores = 0,
        short NumberOfLogicalProcessors = 0
    )
    {
        public string SpecificationText => $"Ядер {NumberOfCores}, Потоков {NumberOfLogicalProcessors}";
    }
}
