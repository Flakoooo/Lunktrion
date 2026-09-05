using LunktrionApp.Hubs;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using System;
using System.Collections.Generic;
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

        public async Task<List<DeviceCPUInfo>> GetDeviceCPUInfoAsync()
        {
            if (_lastCPURefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshCPUList();
                _lastCPURefreshTime = DateTime.Now;
            }

            var cpus = new List<DeviceCPUInfo>();

            foreach (var cpu in _hardwareService.Hardware.CpuList)
            {
                cpus.Add(cpu is null
                    ? new DeviceCPUInfo()
                    : new DeviceCPUInfo(
                        cpu.Name,
                        (short)cpu.NumberOfCores,
                        (short)cpu.NumberOfLogicalProcessors
                    )
                );
            }

            return cpus;
        }

        public async Task<List<DeviceGPUInfo>> GetDeviceGPUInfoAsync()
        {
            if (_lastGPURefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshVideoControllerList();
                _lastGPURefreshTime = DateTime.Now;
            }

            var gpus = new List<DeviceGPUInfo>();

            foreach (var gpu in _hardwareService.Hardware.VideoControllerList)
            {
                gpus.Add(gpu is null
                    ? new DeviceGPUInfo()
                    : new DeviceGPUInfo(
                        gpu.Name,
                        gpu.AdapterRAM
                    )
                );
            }

            return gpus;
        }

        public async Task<List<DeviceRAMInfo>> GetDeviceRAMInfoAsync()
        {
            if (_lastRAMRefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshMemoryStatus();
                await _hardwareService.RefreshMemoryList();
                _lastRAMRefreshTime = DateTime.Now;
            }

            var rams = new List<DeviceRAMInfo>();

            foreach (var ram in _hardwareService.Hardware.MemoryList)
            {
                rams.Add(ram is null
                    ? new DeviceRAMInfo()
                    : new DeviceRAMInfo(
                        ram.Manufacturer,
                        ram.Capacity,
                        ram.MemoryType.ToString(),
                        ram.Speed
                    )
                );
            }


            return rams;
        }

        public async Task<List<DeviceDriveInfo>> GetDeviceDriveInfoAsync()
        {
            if (_lastDriveRefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshDriveList();
                _lastDriveRefreshTime = DateTime.Now;
            }

            var drivers = new List<DeviceDriveInfo>();

            foreach (var drive in _hardwareService.Hardware.DriveList)
            {
                drivers.Add(drive is null
                    ? new DeviceDriveInfo()
                    : new DeviceDriveInfo(
                        drive.Caption,
                        drive.Size
                    )
                );
            }

            return drivers;
        }

        public async void OnDeviceInfoRequestReceived(DeviceInfoRequest request)
        {
            var cpuInfo = await GetDeviceCPUInfoAsync();
            var gpuInfo = await GetDeviceGPUInfoAsync();
            var ramInfo = await GetDeviceRAMInfoAsync();
            var driveInfo = await GetDeviceDriveInfoAsync();

            var response = new DeviceInfoResponse(
                request.TargetDeviceId,
                request.RequestorDeviceId,
                cpuInfo,
                gpuInfo,
                ramInfo,
                driveInfo
            );

            await _mainHub.SendDeviceInfoAsync(response);
        }

        public void Dispose()
        {
            _mainHub.DeviceInfoRequestReceived -= OnDeviceInfoRequestReceived;
        }
    }
}
