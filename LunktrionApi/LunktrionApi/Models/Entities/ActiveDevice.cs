using LunktrionShared.Models.Enums;

namespace LunktrionApi.Models.Entities
{
    public record class ActiveDevice(
        Guid DeviceId,
        string DeviceUUID, 
        OperatingSystemType OperatingSystemType,
        string ConnectionId, 
        DateTime ConnectedAt
    ) 
    {
        public ActiveDevice(
            Guid DeviceId, string DeviceUUID, OperatingSystemType OperatingSystemType, string ConnectionId
        ) : this(DeviceId, DeviceUUID, OperatingSystemType, ConnectionId, DateTime.Now) { }

        public override int GetHashCode()
            => DeviceId.GetHashCode();
    }
}
