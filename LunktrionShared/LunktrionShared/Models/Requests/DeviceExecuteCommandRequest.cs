namespace LunktrionShared.Models.Requests
{
    public record class DeviceExecuteCommandRequest(
        string TargetDeviceId,
        string RequestorDeviceId,
        string Command,
        DateTime RequestedAt
    )
    {
        public DeviceExecuteCommandRequest(
            string TargetDeviceId, string RequestorDeviceId, string Command
        ) : this(TargetDeviceId, RequestorDeviceId, Command, DateTime.Now) { }
    }
}
