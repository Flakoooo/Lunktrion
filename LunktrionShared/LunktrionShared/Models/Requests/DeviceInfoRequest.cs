namespace LunktrionShared.Models.Requests
{
    public record class DeviceInfoRequest(
        string TargetDeviceId,
        string RequestorDeviceId
    );
}
