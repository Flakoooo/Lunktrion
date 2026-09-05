namespace LunktrionShared.Models.DTOs
{
    public record class DeviceInfoDTO(
        List<DeviceCPUInfo> CPUInfos,
        List<DeviceGPUInfo> GPUInfos,
        List<DeviceRAMInfo> RAMInfos,
        List<DeviceDriveInfo> DriveInfos
    );
}
