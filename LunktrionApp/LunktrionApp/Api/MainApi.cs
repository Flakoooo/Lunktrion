using LunktrionApp.Services;
using LunktrionApp.ViewModels;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LunktrionApp.Api
{
    public class MainApi(
        IHttpClientFactory httpClientFactory,
        NotificationService notificationService
    )
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly NotificationService _notificationService = notificationService;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly Uri _apiPath = new($"{BuildConfig.ApiBaseUrl}/api/");

        public async Task<bool> CheckDeviceOnlineStatusAsync(string deviceId)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = _apiPath;

                var response = await client.GetAsync($"v1/device/online/{deviceId}");

                if (response.StatusCode is System.Net.HttpStatusCode.OK)
                    return true;

                if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
                    return false;

                _notificationService.ShowError($"[{response.StatusCode}] Не удалось получить статус подключения");
                return false;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(ex.Message);
                return false;
            }
        }

        public async Task<bool> VerifyCodeAsync(ushort code)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = _apiPath;

                var response = await client.PostAsync(
                    "v1/device/verify/code",
                    JsonContent.Create(new VerifyCodeRequest(code), options: _jsonOptions)
                );

                if (response.StatusCode is System.Net.HttpStatusCode.OK)
                    return true;

                if (response.StatusCode is System.Net.HttpStatusCode.Forbidden)
                    return false;

                _notificationService.ShowError($"[{response.StatusCode}] Не удалось проверить код");
                return false;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(ex.Message);
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
                    _notificationService.ShowError($"[{response.StatusCode}] Не удалось получить все устройства");
                    return [];
                }

                var content = await response.Content.ReadAsStringAsync();
                var devices = JsonSerializer.Deserialize<IEnumerable<DeviceIdentity>>(content, _jsonOptions);

                return devices ?? [];
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(ex.Message);
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
                    _notificationService.ShowError($"[{response.StatusCode}] Не удалось получить информацию о устройстве");
                    return null;
                }

                var conent = await response.Content.ReadAsStringAsync();
                var info = JsonSerializer.Deserialize<DeviceInfoDTO>(conent, _jsonOptions);

                return info;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(ex.Message);
                return null;
            }
        }
    }
}
