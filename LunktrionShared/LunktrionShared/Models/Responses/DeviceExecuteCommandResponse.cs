namespace LunktrionShared.Models.Responses
{
    public record class DeviceExecuteCommandResponse(
        string Command,
        string Output,
        string TargetDeviceId,
        string RequestorDeviceId
    );
}
