using LunktrionShared.Models.Enums;

namespace LunktrionShared.Models.Entities
{
    public record class DeviceIdentity(
        string DeviceUUID = "ОШИБКА", 
        string DeviceName = "ОШИБКА",
        OperatingSystemType OperatingSystemType = OperatingSystemType.Unknown,
        string OperatingSystemName = "ОШИБКА", 
        string DeviceManufacturer = "ОШИБКА"
    )
    {
        public override int GetHashCode()
            => DeviceUUID.GetHashCode();
    }
}
