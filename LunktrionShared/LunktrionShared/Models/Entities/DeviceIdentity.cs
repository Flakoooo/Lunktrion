namespace LunktrionShared.Models.Entities
{
    public record class DeviceIdentity(
        string DeviceId = "ОШИБКА", 
        string DeviceName = "ОШИБКА", 
        string OperatingSystemName = "ОШИБКА", 
        string DeviceManufacturer = "ОШИБКА"
    )
    {
        public override int GetHashCode()
            => DeviceId.GetHashCode();
    }
}
