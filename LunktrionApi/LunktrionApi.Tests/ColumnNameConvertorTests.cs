using LunktrionApi.Utils;

namespace LunktrionApi.Tests
{
    public class ColumnNameConvertorTests
    {
        [Test]
        public void ConvertNameToSnakeCase_Should_ReturnEmptyString() => Assert.That(
            Converters.ConvertNameToSnakeCase(string.Empty),
            Is.Empty
        );

        [TestCase("DeviceId", ExpectedResult = "device_id")]
        [TestCase("DeviceUUID", ExpectedResult = "device_uuid")]
        public string ConvertNameToSnakeCase_Should_ReturnCorrectResult(string name)
            => Converters.ConvertNameToSnakeCase(name);
    }
}
