using LunktrionShared.Models.DTOs;

namespace LunktrionShared.Models.Responses
{
    public record class DeviceInfoResponse(
        string TargetDeviceId,
        string RequestorDeviceId,
        List<DeviceCPUInfo> CPUInfos,
        List<DeviceGPUInfo> GPUInfos,
        List<DeviceRAMInfo> RAMInfos,
        List<DeviceDriveInfo> DriveInfos
    );
}
