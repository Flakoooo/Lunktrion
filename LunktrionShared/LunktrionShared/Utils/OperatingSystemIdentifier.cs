using LunktrionShared.Models.Enums;

namespace LunktrionShared.Utils
{
    public static class OperatingSystemIdentifier
    {
        private static readonly Dictionary<string, OperatingSystemType> OsMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Windows"] = OperatingSystemType.Windows,
            ["Linux"] = OperatingSystemType.Linux,
            ["MacOS"] = OperatingSystemType.MacOS,
            ["Android"] = OperatingSystemType.Android,
            ["IOS"] = OperatingSystemType.IOS
        };

        public static OperatingSystemType Check(string operatingSystemName)
        {
            foreach (var pair in OsMappings)
            {
                if (operatingSystemName.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }

            return OperatingSystemType.Unknown;
        }
    }
}
