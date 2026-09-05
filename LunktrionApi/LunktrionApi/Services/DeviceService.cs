using LunktrionApi.Data;
using LunktrionApi.Models.Entities;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using LunktrionShared.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace LunktrionApi.Services
{
    public class DeviceService(
        IDbContextFactory<AppDbContext> dbFactory,
        RedisService redisService, 
        ILogger<DeviceService> logger
    )
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        private readonly RedisService _redisService = redisService;
        private readonly ILogger<DeviceService> _logger = logger;

        /// <summary>
        /// Активные устройства в данный момент подключения
        /// </summary>
        private readonly ConcurrentDictionary<string, ActiveDevice> _activeDevices = new();

        // при подключении НОВОГО устройства, создаются записи в бд с их информацией (для процессора частота не сохраняется)
        // для ОБНОВЛЕНИЯ ифнрмации, нужно нажать соотвествующую кнопку на клиенте
        // (будет запрос на нужное устройство для получения новой информации)
        // а при переподключении будет сравниваться спецификация на корректность

        // зачем тогда фоновый сервис?

        public async Task<bool> Register(RegisterDeviceReuest request, string connectionId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var existedDevice = await db.Devices.FirstOrDefaultAsync(
                d => d.DeviceUUID == request.Identity.DeviceUUID
            );

            if (existedDevice is null)
            {
                var newDevice = new Device
                {
                    DeviceUUID = request.Identity.DeviceUUID,
                    DeviceName = request.Identity.DeviceName,
                    OperatingSystemType = OperatingSystemIdentifier.Check(
                        request.Identity.OperatingSystemName
                    ),
                    OperatingSystemName = request.Identity.OperatingSystemName,
                    DeviceManufacturer = request.Identity.DeviceManufacturer,
                    CpuSpecifications = request.CPUInfos.Select(i => new DeviceCpuSpecification
                    {
                        Name = i.Name,
                        NumberOfCores = i.NumberOfCores,
                        NumberOfLogicalProcessors = i.NumberOfLogicalProcessors
                    }).ToList(),
                    GpuSpecifications = request.GPUInfos.Select(i => new DeviceGpuSpecification
                    {
                        Name = i.Name,
                        VideoRam = i.VideoRAM
                    }).ToList(),
                    RamSpecifications = request.RAMInfos.Select(i => new DeviceRamSpecification
                    {
                        Manufacturer = i.Manufacturer,
                        Size = i.Size,
                        Type = i.Type,
                        Speed = i.Speed
                    }).ToList(),
                    DriveSpecifications = request.DriveInfos.Select(i => new DeviceDriveSpecification
                    {
                        Caption = i.Caption,
                        TotalSize = i.TotalSize
                    }).ToList()
                };

                db.Devices.Add(newDevice);
                await db.SaveChangesAsync();

                existedDevice = newDevice;

                if (existedDevice is not null)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Устройство {DeviceId} добавлено в реестр всех устройств", request.Identity.DeviceUUID);
                    }
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Не удалось добавить устройство {DeviceId} в реестр всех устройств", request.Identity.DeviceUUID);
                    }

                    return false;
                }
            }

            var existingActiveDevice = _activeDevices.Values.FirstOrDefault(d => Equals(d.DeviceId, existedDevice.Id));
            if (existingActiveDevice is not null)
                _activeDevices.TryRemove(existingActiveDevice.ConnectionId, out _);

            var newActiveDevice = new ActiveDevice(
                existedDevice.Id,
                existedDevice.DeviceUUID,
                existedDevice.OperatingSystemType,
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

        public ActiveDevice? RemoveActiveDevice(string connectionId)
        {
            _activeDevices.TryRemove(connectionId, out var device);
            return device;
        }

        public async Task<bool> UpdateDeviceInfo(DeviceInfoResponse response)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var deviceId = await db.Devices.Where(
                d => d.DeviceUUID == response.TargetDeviceId
            ).Select(d => (Guid?)d.Id).FirstOrDefaultAsync();

            if (deviceId.HasValue)
            {
                await using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    await db.Database.ExecuteSqlAsync($@"
                        DELETE FROM {DeviceCpuSpecification.TableName} 
                        WHERE {DeviceCpuSpecification.DeviceIdColumnName} = {deviceId.Value}
                    ");

                    await db.Database.ExecuteSqlAsync($@"
                        DELETE FROM {DeviceGpuSpecification.TableName} 
                        WHERE {DeviceGpuSpecification.DeviceIdColumnName} = {deviceId.Value}
                    ");

                    await db.Database.ExecuteSqlAsync($@"
                        DELETE FROM {DeviceRamSpecification.TableName} 
                        WHERE {DeviceRamSpecification.DeviceIdColumnName} = {deviceId.Value}
                    ");

                    await db.Database.ExecuteSqlAsync($@"
                        DELETE FROM {DeviceDriveSpecification.TableName} 
                        WHERE {DeviceDriveSpecification.DeviceIdColumnName} = {deviceId.Value}
                    ");

                    var newCpuInfos = response.CPUInfos.Select(i => new DeviceCpuSpecification
                    {
                        DeviceId = deviceId.Value,
                        Name = i.Name,
                        NumberOfCores = i.NumberOfCores,
                        NumberOfLogicalProcessors = i.NumberOfLogicalProcessors
                    }).ToList();

                    var newGpuInfos = response.GPUInfos.Select(i => new DeviceGpuSpecification
                    {
                        DeviceId = deviceId.Value,
                        Name = i.Name,
                        VideoRam = i.VideoRAM
                    }).ToList();

                    var newRamInfos = response.RAMInfos.Select(i => new DeviceRamSpecification
                    {
                        DeviceId = deviceId.Value,
                        Manufacturer = i.Manufacturer,
                        Size = i.Size,
                        Type = i.Type,
                        Speed = i.Speed
                    }).ToList();

                    var newDriveInfos = response.DriveInfos.Select(i => new DeviceDriveSpecification
                    {
                        DeviceId = deviceId.Value,
                        Caption = i.Caption,
                        TotalSize = i.TotalSize
                    }).ToList();

                    db.AddRange(newCpuInfos, newGpuInfos, newRamInfos, newDriveInfos);
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Не удалось обновить информацию об устройстве {DeviceId}",
                    response.TargetDeviceId
                );
            }

            return false;
        }

        public async Task<IReadOnlyCollection<DeviceIdentity>> GetAllDevicesAsync()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var devices = await db.Devices.Select(
                d => new DeviceIdentity(
                    d.DeviceUUID, d.DeviceName, d.OperatingSystemType, d.OperatingSystemName, d.DeviceManufacturer
                )
            ).ToListAsync();

            return devices.AsReadOnly();
        }

        public async Task<DeviceIdentity?> GetDeviceByDeviceId(string targetDeviceId)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Поиск устройства {DeviceId}", targetDeviceId);

            await using var db = await _dbFactory.CreateDbContextAsync();
            
            var device = await db.Devices.FirstOrDefaultAsync(
                d => d.DeviceUUID == targetDeviceId
            );

            return device is not null ? new DeviceIdentity
            {
                DeviceUUID = device.DeviceUUID,
                DeviceName = device.DeviceName,
                OperatingSystemName = device.OperatingSystemName,
                DeviceManufacturer = device.DeviceManufacturer
            } : null;
        }

        public ActiveDevice? GetActiveDeviceByDeviceId(string targetDeviceId)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Поиск активного устройства {DeviceId}", targetDeviceId);

            return _activeDevices.Values.FirstOrDefault(
                d => string.Equals(d.DeviceUUID, targetDeviceId, StringComparison.Ordinal)
            );
        }

        public async Task<DeviceInfoDTO?> GetDeviceInfoAsync(string deviceId)
        {
            var cachedInfo = await _redisService.GetDeviceInfoAsync(deviceId);
            if (cachedInfo is not null)
            {
                return cachedInfo;
            }

            await using var db = await _dbFactory.CreateDbContextAsync();

            var deviceGuid = await db.Devices
                .Where(d => d.DeviceUUID == deviceId)
                .AsNoTracking()
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync();

            if (!deviceGuid.HasValue)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Не удалось найти устройство с UUID равным {DviceUUID}", deviceId);
                }

                return null;
            }

            var cpuTask = db.DeviceCpuSpecifications
                .Where(dcs => dcs.DeviceId == deviceGuid.Value)
                .AsNoTracking()
                .Select(dcs => new DeviceCPUInfo(dcs.Name, dcs.NumberOfCores, dcs.NumberOfLogicalProcessors))
                .ToListAsync();

            var gpuTask = db.DeviceGpuSpecifications
                .Where(dgs => dgs.DeviceId == deviceGuid.Value)
                .AsNoTracking()
                .Select(dgs => new DeviceGPUInfo(dgs.Name, dgs.VideoRam))
                .ToListAsync();

            var ramTask = db.DeviceRamSpecifications
                .Where(drs => drs.DeviceId == deviceGuid.Value)
                .AsNoTracking()
                .Select(drs => new DeviceRAMInfo(drs.Manufacturer, drs.Size, drs.Type, drs.Speed))
                .ToListAsync();

            var driveTask = db.DeviceDriveSpecifications
                .Where(dds => dds.DeviceId == deviceGuid.Value)
                .AsNoTracking()
                .Select(dds => new DeviceDriveInfo(dds.Caption, dds.TotalSize))
                .ToListAsync();

            await Task.WhenAll(cpuTask, gpuTask, ramTask, driveTask);

            var deviceInfoDto = new DeviceInfoDTO(
                cpuTask.Result,
                gpuTask.Result,
                ramTask.Result,
                driveTask.Result
            );

            await _redisService.SetDeviceInfoAsync(deviceId, deviceInfoDto);

            return deviceInfoDto;
        }

        public async Task<DeviceExecuteCommandResponse?> TryGetCachedDeviceExecuteCommandResponseAsync(string targetDeviceId)
            => await _redisService.GetDeviceExecuteCommandResponseAsync(targetDeviceId);

        public async Task SetDeviceExecuteCommandResponseInCacheAsync(DeviceExecuteCommandResponse response)
            => await _redisService.SetCommandOutputAsync(response);
    }
}
