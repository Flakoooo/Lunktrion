using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LunktrionApp.Api
{
    public class MainApi(
        IHttpClientFactory httpClientFactory
    )
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly Uri _apiPath = new($"{BuildConfig.ApiBaseUrl}/api/");

        public event Action<string>? ErrorReceived;

        public async Task<bool> GetDeviceOnlineStatusAsync(string deviceId)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = _apiPath;

                var response = await client.GetAsync($"v1/device/online/{deviceId}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorReceived?.Invoke($"[{response.StatusCode}] Не удалось получить статус подключения");
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                var online = JsonSerializer.Deserialize<DeviceOnlineResponse>(content, _jsonOptions);

                return online?.IsOnline ?? false;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(ex.Message);
                return false;
            }
        }

        public async Task<IEnumerable<DeviceIdentity>> GetAllDevicesAsync()
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = _apiPath;

                var response = await client.GetAsync("v1/device");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorReceived?.Invoke($"[{response.StatusCode}] Не удалось получить все устройства");
                    return [];
                }

                var content = await response.Content.ReadAsStringAsync();
                var devices = JsonSerializer.Deserialize<IEnumerable<DeviceIdentity>>(content, _jsonOptions);

                return devices ?? [];
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(ex.Message);
                return [];
            }
        }

        public async Task<DeviceInfoDTO?> GetDeviceInfoAsync(string deviceId)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = _apiPath;

                var response = await client.GetAsync($"v1/device/{deviceId}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorReceived?.Invoke($"[{response.StatusCode}] Не удалось получить информацию о устройстве");
                    return null;
                }

                var conent = await response.Content.ReadAsStringAsync();
                var info = JsonSerializer.Deserialize<DeviceInfoDTO>(conent, _jsonOptions);

                return info;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(ex.Message);
                return null;
            }
        }
    }
}
