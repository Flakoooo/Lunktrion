using LunktrionShared.Models.Enums;
using LunktrionShared.Models.Interfaces;

namespace LunktrionApi.Models.Entities
{
    public record class ActiveDevice(
        Guid DeviceId,
        string DeviceUUID, 
        OperatingSystemType OperatingSystemType,
        string ConnectionId, 
        DateTime ConnectedAt,
        bool WaitingForShutdown = false
    ) : IDevice
    {
        public bool WaitingForShutdown { get; set; } = WaitingForShutdown;

        public ActiveDevice(
            Guid DeviceId, string DeviceUUID, OperatingSystemType OperatingSystemType, 
            string ConnectionId, bool WaitingForShutdown = false
        ) : this(DeviceId, DeviceUUID, OperatingSystemType, ConnectionId, DateTime.Now, WaitingForShutdown) { }

        public override int GetHashCode()
            => DeviceId.GetHashCode();
    }
}
