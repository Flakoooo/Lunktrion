using LunktrionApp.Models.Entities;
using LunktrionShared.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LunktrionApp.Services
{
    public class DeviceIdentityService
    {
        private readonly HardwareService _hardwareService;
        private DateTime _lastRefreshTime = DateTime.MinValue;

        public DeviceIdentityService(HardwareService hardwareService)
        {
            _hardwareService = hardwareService;
        }

        public DeviceIdentityService()
        {
            _hardwareService = null!;
        }

        public async Task<DeviceIdentity> GetCurrentDeviceAsync()
        {
            if (_lastRefreshTime.AddMinutes(5) < DateTime.Now)
            {
                await _hardwareService.RefreshComputerSystemList();
                await _hardwareService.RefreshOperatingSystem();
                _lastRefreshTime = DateTime.Now;
            }

            var computerSystem = _hardwareService.Hardware.ComputerSystemList.FirstOrDefault();

            return computerSystem is null
                ? new DeviceIdentity(
                    OperatingSystemName: _hardwareService.Hardware.OperatingSystem.Name
                )
                : new DeviceIdentity(
                    computerSystem.UUID, 
                    computerSystem.Name, 
                    _hardwareService.Hardware.OperatingSystem.Name, 
                    computerSystem.Vendor
                );
        }
    }
}
