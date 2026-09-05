using LunktrionApi.Utils;

namespace LunktrionApi.Models.Entities
{
    public class DeviceGpuSpecification
    {
        public static readonly string TableName = $"{Converters.ConvertNameToSnakeCase(nameof(DeviceGpuSpecification))}s";
        public static readonly string DeviceIdColumnName = Converters.ConvertNameToSnakeCase(nameof(DeviceId));

        public Guid Id { get; set; }

        public Guid DeviceId { get; set; }
        public Device? Device { get; set; }

        public required string Name { get; set; } 
        public ulong VideoRam { get; set; }
    }
}
