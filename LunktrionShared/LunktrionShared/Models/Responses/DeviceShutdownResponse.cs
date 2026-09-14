namespace LunktrionShared.Models.Responses
{
    public record class DeviceShutdownResponse(
        string TargetDeviceId,
        string RequestorDeviceId,
        DateTime ShutdownAt
    );
}
