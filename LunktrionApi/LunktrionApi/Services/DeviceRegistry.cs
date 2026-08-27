using LunktrionApi.Models.Entities;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Responses;
using System.Collections.Concurrent;

namespace LunktrionApi.Services
{
    public class DeviceRegistry(
        RedisService redisService, 
        ILogger<DeviceRegistry> logger
    )
    {
        private readonly RedisService _redisService = redisService;
        private readonly ILogger<DeviceRegistry> _logger = logger;

        /// <summary>
        /// Все устройства, которые когда либо подключались
        /// </summary>
        private readonly ConcurrentDictionary<string, DeviceIdentity> _allDevices = new();

        /// <summary>
        /// Активные устройства в данный момент подключения
        /// </summary>
        private readonly ConcurrentDictionary<string, ActiveDevice> _activeDevices = new();

        public bool Register(DeviceIdentity device, string connectionId)
        {
            if (!_allDevices.ContainsKey(device.DeviceId))
            {
                var newIdentity = new DeviceIdentity 
                {
                    DeviceId = device.DeviceId,
                    DeviceName = device.DeviceName,
                    DeviceManufacturer = device.DeviceManufacturer,
                    OperatingSystemName = device.OperatingSystemName
                };

                var allDevicesRegisterResult = _allDevices.TryAdd(device.DeviceId, newIdentity);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    if (allDevicesRegisterResult)
                    {
                        _logger.LogInformation("Устройство {DeviceId} добавлено в реестр всех устройств", newIdentity.DeviceId);
                    }
                    else
                    {
                        _logger.LogInformation("Не удалось добавить устройство {DeviceId} в реестр всех устройств", newIdentity.DeviceId);
                    }
                }
            }

            var existingActiveDevice = _activeDevices.Values.FirstOrDefault(d => d.DeviceId.Equals(device.DeviceId));
            if (existingActiveDevice is not null)
                _activeDevices.TryRemove(existingActiveDevice.ConnectionId, out _);

            var newActiveDevice = new ActiveDevice(
                device.DeviceId, 
                device.DeviceName, 
                device.OperatingSystemName, 
                device.DeviceManufacturer, 
                connectionId
            );

            var activeDevicesRegisterResult = _activeDevices.TryAdd(connectionId, newActiveDevice);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                if (activeDevicesRegisterResult)
                {
                    _logger.LogInformation("Устройство {DeviceId} добавлено в реестр активных устройств", newActiveDevice.DeviceId);
                }
                else
                {
                    _logger.LogInformation("Не удалось добавить устройство {DeviceId} в реестр активных устройств", newActiveDevice.DeviceId);
                }
            }

            return activeDevicesRegisterResult;
        }

        public ActiveDevice? Remove(string connectionId)
        {
            _activeDevices.TryRemove(connectionId, out var device);
            return device;
        }

        public IReadOnlyCollection<DeviceIdentity> GetAllDevices() 
            => _allDevices.Values.Select(d => new DeviceIdentity(
                d.DeviceId, 
                d.DeviceName, 
                d.DeviceManufacturer, 
                d.OperatingSystemName
            ))
            .ToList().AsReadOnly();

        public IReadOnlyCollection<DeviceIdentity> GetAllAcitveDevices()
            => _activeDevices.Values.Select(d => new DeviceIdentity(
                d.DeviceId,
                d.DeviceName,
                d.DeviceManufacturer,
                d.OperatingSystemName
            ))
            .ToList().AsReadOnly();

        public DeviceIdentity? GetDeviceByDeviceId(string targetDeviceId)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Поиск устройства {DeviceId}", targetDeviceId);

            return _allDevices.GetValueOrDefault(targetDeviceId);
        }

        public ActiveDevice? GetActiveDeviceByDeviceId(string targetDeviceId)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Поиск активного устройства {DeviceId}", targetDeviceId);

            return _activeDevices.Values.FirstOrDefault(d => d.DeviceId.Equals(targetDeviceId));
        }

        public async Task<DeviceInfo?> TryGetCachedDeviceInfoAsync(string targetDeviceId)
            => await _redisService.GetDeviceInfoAsync(targetDeviceId);

        public async Task<DeviceExecuteCommandResponse?> TryGetCachedDeviceExecuteCommandResponseAsync(string targetDeviceId)
            => await _redisService.GetDeviceExecuteCommandResponseAsync(targetDeviceId);

        public async Task SetDeviceInfoInCacheAsync(DeviceInfoResponse response)
            => await _redisService.SetDeviceInfoAsync(response);

        public async Task SetDeviceExecuteCommandResponseInCacheAsync(DeviceExecuteCommandResponse response)
            => await _redisService.SetCommandOutputAsync(response);
    }
}
