namespace LunktrionShared.Models.Requests
{
    public record class DeviceShutdownRequest(
        string TargetDeviceId,
        string RequestorDeviceId,
        ushort? Code
    );
}
