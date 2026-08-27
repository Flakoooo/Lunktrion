using LunktrionShared.Models.Entities;

namespace LunktrionApi.Models.Entities
{
    public record class ActiveDevice(
        string DeviceId, string DeviceName, string OperatingSystemName, string DeviceManufacturer,
        string ConnectionId, DateTime ConnectedAt
    ) : DeviceIdentity(
        DeviceId, DeviceName, OperatingSystemName, DeviceManufacturer
    )
    {
        public ActiveDevice(
            string DeviceId, string DeviceName, string OperatingSystemName, string DeviceManufacturer, 
            string ConnectionId
        ) : this(
            DeviceId, DeviceName, OperatingSystemName, DeviceManufacturer, 
            ConnectionId, DateTime.Now
        ) { }

        public override int GetHashCode()
            => DeviceId.GetHashCode();
    }
}
