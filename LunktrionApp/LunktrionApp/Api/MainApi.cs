using LunktrionShared.Models.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LunktrionApp.Api
{
    public class MainApi(
        HttpClient httpClient
    )
    {
        private readonly HttpClient _httpClient = httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<IEnumerable<DeviceIdentity>> GetAllDevices()
        {
            var response = await _httpClient.GetAsync("v1/device");

            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var devices = JsonSerializer.Deserialize<IEnumerable<DeviceIdentity>>(content, _jsonOptions);

            return devices ?? [];
        }
    }
}
