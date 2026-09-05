using LunktrionApi.Utils;

namespace LunktrionApi.Models.Entities
{
    public class DeviceDriveSpecification
    {
        public static readonly string TableName = $"{Converters.ConvertNameToSnakeCase(nameof(DeviceDriveSpecification))}s";
        public static readonly string DeviceIdColumnName = Converters.ConvertNameToSnakeCase(nameof(DeviceId));

        public Guid Id { get; set; }

        public Guid DeviceId { get; set; }
        public Device? Device { get; set; }

        public required string Caption { get; set; }
        public ulong TotalSize { get; set; }
    }
}
