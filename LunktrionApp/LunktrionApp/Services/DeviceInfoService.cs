using LunktrionApp.Hubs;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LunktrionApp.Services
{
    public class DeviceInfoService : IDisposable
    {
        private readonly MainHub _mainHub;
        private readonly HardwareService _hardwareService;

        private DateTime _lastCPURefreshTime = DateTime.MinValue;
        private DateTime _lastGPURefreshTime = DateTime.MinValue;
        private DateTime _lastRAMRefreshTime = DateTime.MinValue;
        private DateTime _lastDriveRefreshTime = DateTime.MinValue;

        public DeviceInfoService(MainHub mainHub,HardwareService hardwareService)
        {
            _mainHub = mainHub;
            _hardwareService = hardwareService;

            _mainHub.DeviceInfoRequestReceived += OnDeviceInfoRequestReceived;
        }

        public DeviceInfoService()
        {
            _mainHub = null!;
            _hardwareService = null!;
        }

        public async Task<DeviceCPUInfo> GetDeviceCPUInfoAsync()
        {
            if (_lastCPURefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshCPUList();
                _lastCPURefreshTime = DateTime.Now;
            }

            var cpu = _hardwareService.Hardware.CpuList.FirstOrDefault();

            return cpu is null
                ? new DeviceCPUInfo()
                : new DeviceCPUInfo(
                    cpu.Name, 
                    cpu.NumberOfCores, 
                    cpu.NumberOfLogicalProcessors, 
                    cpu.CurrentClockSpeed, 
                    cpu.MaxClockSpeed
                );
        }

        public async Task<DeviceGPUInfo> GetDeviceGPUInfoAsync()
        {
            if (_lastGPURefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshVideoControllerList();
                _lastGPURefreshTime = DateTime.Now;
            }

            var gpu = _hardwareService.Hardware.VideoControllerList.FirstOrDefault();

            return gpu is null
                ? new DeviceGPUInfo()
                : new DeviceGPUInfo(
                    gpu.Name, 
                    gpu.AdapterRAM, 
                    gpu.MaxRefreshRate
                );
        }

        public async Task<DeviceRAMInfo> GetDeviceRAMInfoAsync()
        {
            if (_lastRAMRefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshMemoryStatus();
                await _hardwareService.RefreshMemoryList();
                _lastRAMRefreshTime = DateTime.Now;
            }

            var ram = _hardwareService.Hardware.MemoryList.FirstOrDefault();

            return ram is null
                ? new DeviceRAMInfo()
                : new DeviceRAMInfo(
                    _hardwareService.Hardware.MemoryList.Aggregate(0UL, (sum, next) => sum + next.Capacity),
                    _hardwareService.Hardware.MemoryStatus.TotalPhysical,
                    ram.MemoryType.ToString(),
                    ram.Speed
                );
        }

        public async Task<DeviceDriveInfo> GetDeviceDriveInfoAsync()
        {
            if (_lastDriveRefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshDriveList();
                _lastDriveRefreshTime = DateTime.Now;
            }

            var partitions = _hardwareService.Hardware.DriveList.SelectMany(d => d.PartitionList);

            return new DeviceDriveInfo(
                (uint)_hardwareService.Hardware.DriveList.Count,
                partitions.Aggregate(0UL, (sum, next) => sum + next.Size),
                partitions.SelectMany(p => p.VolumeList).Aggregate(0UL, (sum, next) => sum + next.FreeSpace)
            );
        }

        public async void OnDeviceInfoRequestReceived(DeviceInfoRequest request)
        {
            var cpuInfo = await GetDeviceCPUInfoAsync();
            var gpuInfo = await GetDeviceGPUInfoAsync();
            var ramInfo = await GetDeviceRAMInfoAsync();
            var driveInfo = await GetDeviceDriveInfoAsync();

            var response = new DeviceInfoResponse(
                new DeviceInfo(
                    cpuInfo, 
                    gpuInfo, 
                    ramInfo, 
                    driveInfo
                ), 
                request.TargetDeviceId,
                request.RequestorDeviceId
            );

            await _mainHub.SendDeviceInfoAsync(response);
        }

        public void Dispose()
        {
            _mainHub.DeviceInfoRequestReceived -= OnDeviceInfoRequestReceived;
        }
    }
}
