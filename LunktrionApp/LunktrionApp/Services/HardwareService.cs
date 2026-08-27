using Hardware.Info;
using System.Threading.Tasks;

namespace LunktrionApp.Services
{
    public class HardwareService
    {
        private readonly HardwareInfo _hardware = new();

        public HardwareInfo Hardware => _hardware;

        public async Task RefreshAll() 
            => await Task.Run(_hardware.RefreshAll);

        public async Task RefreshBatteryList()
            => await Task.Run(_hardware.RefreshBatteryList);

        public async Task RefreshBIOSList()
            => await Task.Run(_hardware.RefreshBIOSList);

        public async Task RefreshComputerSystemList()
            => await Task.Run(_hardware.RefreshComputerSystemList);

        public async Task RefreshCPUList(
            bool includePercentProcessorTime = true, 
            int millisecondsDelayBetweenTwoMeasurements = 500, 
            bool includePerformanceCounter = true
        ) => await Task.Run(() => _hardware.RefreshCPUList(
                includePercentProcessorTime, 
                millisecondsDelayBetweenTwoMeasurements, 
                includePerformanceCounter
            )
        );

        public async Task RefreshDriveList()
            => await Task.Run(_hardware.RefreshDriveList);

        public async Task RefreshKeyboardList()
            => await Task.Run(_hardware.RefreshKeyboardList);

        public async Task RefreshMemoryList()
            => await Task.Run(_hardware.RefreshMemoryList);

        public async Task RefreshMemoryStatus()
            => await Task.Run(_hardware.RefreshMemoryStatus);

        public async Task RefreshMonitorList()
            => await Task.Run(_hardware.RefreshMonitorList);

        public async Task RefreshMotherboardList()
            => await Task.Run(_hardware.RefreshMotherboardList);

        public async Task RefreshMouseList()
            => await Task.Run(_hardware.RefreshMouseList);

        public async Task RefreshNetworkAdapterList(
            bool includeBytesPerSec = true, 
            bool includeNetworkAdapterConfiguration = true, 
            int millisecondsDelayBetweenTwoMeasurements = 1000
        ) => await Task.Run(() => _hardware.RefreshNetworkAdapterList(
                includeBytesPerSec, 
                includeNetworkAdapterConfiguration, 
                millisecondsDelayBetweenTwoMeasurements
            )
        );

        public async Task RefreshOperatingSystem()
            => await Task.Run(_hardware.RefreshOperatingSystem);

        public async Task RefreshPrinterList()
            => await Task.Run(_hardware.RefreshPrinterList);

        public async Task RefreshSoundDeviceList()
            => await Task.Run(_hardware.RefreshSoundDeviceList);

        public async Task RefreshVideoControllerList(bool refreshMonitorList = true)
            => await Task.Run(() => _hardware.RefreshVideoControllerList(refreshMonitorList));
    }
}
