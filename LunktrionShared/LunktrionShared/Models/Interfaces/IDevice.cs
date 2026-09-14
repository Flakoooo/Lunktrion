using LunktrionShared.Models.Enums;

namespace LunktrionShared.Models.Interfaces
{
    public interface IDevice
    {
        string DeviceUUID { get; init; }
        OperatingSystemType OperatingSystemType { get; init; }
        bool WaitingForShutdown { get; set; }
    }
}
