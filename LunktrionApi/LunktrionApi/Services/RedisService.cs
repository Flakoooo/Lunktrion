using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Responses;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LunktrionApi.Services
{
    public class RedisService(
        IDistributedCache cache,
        ILogger<RedisService> logger
    )
    {
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<RedisService> _logger = logger;
        private const string DEVICE_INFO_KEY_PREFIX = "device:info:";

        private readonly DistributedCacheEntryOptions redisOptions = new() 
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        private readonly JsonSerializerOptions jsonOptions = new() 
        { 
            PropertyNameCaseInsensitive = true 
        };

        private static string GetDeviceInfoKey(string deviceId) => $"{DEVICE_INFO_KEY_PREFIX}{deviceId}";

        public async Task SetDeviceInfoAsync(string deviceId, DeviceInfo info)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(info);

            await _cache.SetAsync(
                GetDeviceInfoKey(deviceId),
                bytes,
                redisOptions
            );

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Данные о устройстве {DeviceId} сохранены в кэш", deviceId);
        }

        public async Task<DeviceInfo?> GetDeviceInfoAsync(string deviceId)
        {
            var bytes = await _cache.GetAsync(GetDeviceInfoKey(deviceId));

            if (bytes is null || bytes.Length == 0)
                return null;

            return JsonSerializer.Deserialize<DeviceInfo>(bytes, jsonOptions);
        }

        public async Task SetCommandOutputAsync(DeviceExecuteCommandResponse response)
        {



            var bytes = JsonSerializer.SerializeToUtf8Bytes();
        }
    }
}
