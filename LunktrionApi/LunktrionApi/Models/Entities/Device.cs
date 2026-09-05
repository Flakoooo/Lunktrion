using LunktrionApi.Utils;
using LunktrionShared.Models.Enums;

namespace LunktrionApi.Models.Entities
{
    public class Device
    {
        public static readonly string TableName = $"{Converters.ConvertNameToSnakeCase(nameof(Device))}s";
        public static readonly string DeviceUUIDColumnName = Converters.ConvertNameToSnakeCase(nameof(DeviceUUID));
        public static readonly string OperatingSystemTypeColumnName = Converters.ConvertNameToSnakeCase(nameof(OperatingSystemType));

        public Guid Id { get; set; }
        public required string DeviceUUID { get; set; }
        public required string DeviceName { get; set; }
        public required OperatingSystemType OperatingSystemType { get; set; }
        public required string OperatingSystemName { get; set; }
        public required string DeviceManufacturer { get; set; }

        public ICollection<DeviceCpuSpecification> CpuSpecifications { get; set; } = [];
        public ICollection<DeviceGpuSpecification> GpuSpecifications { get; set; } = [];
        public ICollection<DeviceRamSpecification> RamSpecifications { get; set; } = []; 
        public ICollection<DeviceDriveSpecification> DriveSpecifications { get; set; } = [];
    }
}
