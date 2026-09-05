using LunktrionApi.Utils;

namespace LunktrionApi.Models.Entities
{
    public class DeviceRamSpecification
    {
        public static readonly string TableName = $"{Converters.ConvertNameToSnakeCase(nameof(DeviceRamSpecification))}s";
        public static readonly string DeviceIdColumnName = Converters.ConvertNameToSnakeCase(nameof(DeviceId));

        public Guid Id { get; set; }

        public Guid DeviceId { get; set; }
        public Device? Device { get; set; }

        public required string Manufacturer { get; set; }
        public ulong Size { get; set; }
        public required string Type { get; set; }
        public uint Speed { get; set; }
    }
}
