namespace LunktrionShared.Models.DTOs
{
    public record class DeviceInfo(
        DeviceCPUInfo CPUInfo,
        DeviceGPUInfo GPUInfo,
        DeviceRAMInfo RAMInfo,
        DeviceDriveInfo DriveInfo
    );
}
