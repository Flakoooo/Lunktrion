using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;

namespace LunktrionShared.Models.Requests
{
    public record class RegisterDeviceReuest(
        DeviceIdentity Identity,
        List<DeviceCPUInfo> CPUInfos,
        List<DeviceGPUInfo> GPUInfos,
        List<DeviceRAMInfo> RAMInfos,
        List<DeviceDriveInfo> DriveInfos
    )
    {
        public RegisterDeviceReuest() 
            : this(new(), [], [], [], []) { }


        public override int GetHashCode()
            => Identity.DeviceUUID.GetHashCode();
    }
}
