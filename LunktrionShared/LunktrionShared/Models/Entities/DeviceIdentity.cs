using LunktrionShared.Models.Enums;
using LunktrionShared.Models.Interfaces;

namespace LunktrionShared.Models.Entities
{
    public record class DeviceIdentity(
        string DeviceUUID = "ОШИБКА", 
        string DeviceName = "ОШИБКА",
        OperatingSystemType OperatingSystemType = OperatingSystemType.Unknown,
        string OperatingSystemName = "ОШИБКА", 
        string DeviceManufacturer = "ОШИБКА",
        bool WaitingForShutdown = false
    ) : IDevice
    {
        public bool WaitingForShutdown { get; set; } = WaitingForShutdown;

        public override int GetHashCode()
            => DeviceUUID.GetHashCode();
    }
}
