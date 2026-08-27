using LunktrionShared.Models.DTOs;

namespace LunktrionShared.Models.Responses
{
    public record class DeviceInfoResponse(
        DeviceInfo Info,
        string TargetDeviceId,
        string RequestorDeviceId
    );
}
