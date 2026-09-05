using LunktrionShared.Models.Enums;

namespace LunktrionApi.Utils
{
    public static class Converters
    {
        public static string ConvertNameToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            int maxPossibleLength = name.Length * 2;
            Span<char> chars = maxPossibleLength <= 256
                ? stackalloc char[maxPossibleLength]
                : new char[maxPossibleLength];

            int index = 0;
            for (int i = 0; i < name.Length; ++i)
            {
                char current = name[i];

                if (i > 0 && char.IsUpper(current))
                {
                    char previous = name[i - 1];

                    bool isTransitionFromLower = !char.IsUpper(previous);
                    bool isTransitionToContext = i + 1 < name.Length && !char.IsUpper(name[i + 1]);

                    if (isTransitionFromLower || isTransitionToContext)
                    {
                        chars[index++] = '_';
                    }
                }

                chars[index++] = char.ToLowerInvariant(current);
            }

            return chars.Slice(0, index).ToString();
        }

        public static OperatingSystemType ParseOperatingSystem(string? value) 
            => string.IsNullOrWhiteSpace(value) 
            || !Enum.TryParse(value, true, out OperatingSystemType result) 
            ? OperatingSystemType.Unknown
            : result;
    }
}
