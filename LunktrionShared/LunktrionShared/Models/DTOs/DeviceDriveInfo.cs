namespace LunktrionShared.Models.DTOs
{
    public record class DeviceDriveInfo(
        string Caption = "ОШИБКА",
        ulong TotalSize = 0L
    )
    {
        public string SpecificationText => $"Объем {TotalSize / 1024.0 / 1024.0 / 1024.0:F2} GB";
    }
}
