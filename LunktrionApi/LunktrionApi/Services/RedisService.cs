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

        private async Task<T?> GetCachedDataAsync<T>(string key)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "[Redis] Получение данных по ключу: {Key}", key
                );
            }

            var bytes = await _cache.GetAsync(key);

            if (bytes is null || bytes.Length == 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(
                        "[Redis] Не удалось получить данные по ключу: {Key}", key
                    );
                }

                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(bytes, jsonOptions);
            }
            catch (JsonException ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(
                        ex, "[Redis] Ошибка десериализации кэша для ключа {Key} в тип {Type}", 
                        key, typeof(T).Name
                    );
                }

                await _cache.RemoveAsync(key);

                return default;
            }
        }

        private async Task SetDataInCacheAsync<T>(string key, T data, DistributedCacheEntryOptions options)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "[Redis] Сохранение в кэш по ключу {Key}", key
                );
            }

            var bytes = JsonSerializer.SerializeToUtf8Bytes(data, jsonOptions);

            await _cache.SetAsync(key, bytes, options);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "[Redis] Данные сохранены в кэш под ключем {Key}", key
                );
            }
        }

        private async Task DeleteCachedDataAsync(string key)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "[Redis] Удаление данных по ключу {Key}", key
                );
            }

            await _cache.RemoveAsync(key);
        }

        public async Task<DeviceInfoDTO?> GetDeviceInfoAsync(string deviceId)
            => await GetCachedDataAsync<DeviceInfoDTO>(GetDeviceInfoKey(deviceId));

        public async Task<DeviceExecuteCommandResponse?> GetDeviceExecuteCommandResponseAsync(string deviceId)
            => await GetCachedDataAsync<DeviceExecuteCommandResponse>(GetCommandKey(deviceId));

        private async Task SetDataInCacheAsync<T>(
            string key, T data, TimeSpan expirationTime
        ) => await SetDataInCacheAsync(
            key, data,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            }
        );

        public async Task SetDeviceInfoAsync(
            string deviceId, DeviceInfoDTO deviceInfoDTO
        ) => await SetDataInCacheAsync(
            GetDeviceInfoKey(deviceId), deviceInfoDTO, TimeSpan.FromMinutes(5)
        );

        public async Task SetCommandOutputAsync(
            DeviceExecuteCommandResponse response
        ) => await SetDataInCacheAsync(
            GetCommandKey(response.RequestorDeviceId), response, TimeSpan.FromHours(24)
        );

        public async Task DeleteDeviceInfoAsync(string deviceId)
            => await DeleteCachedDataAsync(GetDeviceInfoKey(deviceId));
    }
}
