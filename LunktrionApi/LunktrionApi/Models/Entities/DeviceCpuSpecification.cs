using LunktrionApi.Utils;

namespace LunktrionApi.Models.Entities
{
    public class DeviceCpuSpecification
    {
        public static readonly string TableName = $"{Converters.ConvertNameToSnakeCase(nameof(DeviceCpuSpecification))}s";
        public static readonly string DeviceIdColumnName = Converters.ConvertNameToSnakeCase(nameof(DeviceId));

        public Guid Id { get; set; }

        public Guid DeviceId { get; set; }
        public Device? Device { get; set; }

        public required string Name { get; set; }
        public short NumberOfCores { get; set; }
        public short NumberOfLogicalProcessors { get; set; }
    }
}
