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

        private readonly JsonSerializerOptions jsonOptions = new() 
        { 
            PropertyNameCaseInsensitive = true 
        };

        private static string GetDeviceInfoKey(string deviceId) => $"device:info:{deviceId}";
        private static string GetCommandKey(string deviceId) => $"device.command:{deviceId}";

        public async Task<DeviceInfo?> GetDeviceInfoAsync(string deviceId)
        {
            var bytes = await _cache.GetAsync(GetDeviceInfoKey(deviceId));

            if (bytes is null || bytes.Length == 0)
                return null;

            return JsonSerializer.Deserialize<DeviceInfo>(bytes, jsonOptions);
        }

        public async Task<DeviceExecuteCommandResponse?> GetDeviceExecuteCommandResponseAsync(string deviceId)
        {
            var bytes = await _cache.GetAsync(GetCommandKey(deviceId));

            if (bytes is null || bytes.Length == 0)
                return null;

            return JsonSerializer.Deserialize<DeviceExecuteCommandResponse>(bytes, jsonOptions);
        }

        public async Task SetDeviceInfoAsync(DeviceInfoResponse response)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(response.Info);

            var redisOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            await _cache.SetAsync(
                GetDeviceInfoKey(response.TargetDeviceId),
                bytes,
                redisOptions
            );

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Данные о устройстве {DeviceId} сохранены в кэш", 
                    response.TargetDeviceId
                );
            }
        }

        public async Task SetCommandOutputAsync(DeviceExecuteCommandResponse response)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(response);

            var redisOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };

            await _cache.SetAsync(
                GetCommandKey(response.RequestorDeviceId),
                bytes,
                redisOptions
            );

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Результат выполнения команды для устройства {DeviceId} сохранён в кэш",
                    response.RequestorDeviceId
                );
            }
        }
    }
}
