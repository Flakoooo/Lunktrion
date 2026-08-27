namespace LunktrionShared.Models.DTOs
{
    public record class DeviceDriveInfo(
        uint DriversCount,
        ulong TotalSize,
        ulong AvailableSize
    );
}
