namespace LunktrionApi.Models.DTO
{
    public record class DeferredCommandResultDto(
        string TargetDeviceId,
        string RequestorDeviceId,
        string Command,
        string Output,
        DateTime RequestedAt,
        DateTime ExecutedAt
    );
}
