using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Api;
using LunktrionApp.Hubs;
using LunktrionApp.Models.Interfaces;
using LunktrionApp.Services;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Enums;
using LunktrionShared.Models.Responses;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class DeviceViewModel : ViewModelBase, IDisposable, IAsyncInitializable<DeviceIdentity?>
    {
        private readonly MainHub _mainHub;
        private readonly MainApi _mainApi;
        private readonly NavigationService _navigationService;
        private readonly DeviceIdentityService _identityService;
        private readonly DeviceInfoService _infoService;

        public DeviceIdentity? CurrentDevice { get; set; }

        public bool IsCurrentDevice { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsConnectedText))]
        [NotifyPropertyChangedFor(nameof(IsConnectedColor))]
        public partial bool IsConnected { get; set; }
        public string IsConnectedText => $"{(IsConnected ? "В" : "Не в")} сети";
        public string IsConnectedColor => IsConnected ? "#5FA866" : "#D95C4A";

        public ObservableCollection<DeviceCPUInfo> DeviceCPUInfos { get; set; } = [];
        public ObservableCollection<DeviceGPUInfo> DeviceGPUInfos { get; set; } = [];
        public ObservableCollection<DeviceRAMInfo> DeviceRAMInfos { get; set; } = [];
        public ObservableCollection<DeviceDriveInfo> DeviceDriveInfos { get; set; } = [];


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DeviceCPUSpecifications))]
        public partial DeviceCPUInfo? DeviceCPUInfo { get; set; }
        public string DeviceCPUSpecifications => $"Ядер/Потоков {DeviceCPUInfo?.NumberOfCores}/{DeviceCPUInfo?.NumberOfLogicalProcessors}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DeviceGPUSpecifications))]
        public partial DeviceGPUInfo? DeviceGPUInfo { get; set; }
        public string DeviceGPUSpecifications => $"Объем {DeviceGPUInfo?.VideoRAM / 1024.0 / 1024.0} MB";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DeviceRAMSpecifications))]
        public partial DeviceRAMInfo? DeviceRAMInfo { get; set; }
        public string DeviceRAMSpecifications => $"Тип {DeviceRAMInfo?.Type}, Объем {DeviceRAMInfo?.Size / 1024.0 / 1024.0 / 1024.0} GB " +
            $"Частота {DeviceRAMInfo?.Speed} MHz";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DeviceDriveSpecifications))]
        public partial DeviceDriveInfo? DeviceDriveInfo { get; set; }
        public string DeviceDriveSpecifications => $"Объем/Доступно {DeviceDriveInfo?.TotalSize / 1024.0 / 1024.0 / 1024.0:F2} GB";

        public async Task InitializeAsync(DeviceIdentity? device = null)
        {
            var currentDevice = await _identityService.GetCurrentDeviceAsync();
            if (device is null)
            {
                CurrentDevice = currentDevice;
                IsCurrentDevice = true;

                DeviceCPUInfos = new ObservableCollection<DeviceCPUInfo>(await _infoService.GetDeviceCPUInfoAsync());
                DeviceGPUInfos = new ObservableCollection<DeviceGPUInfo>(await _infoService.GetDeviceGPUInfoAsync());
                DeviceRAMInfos = new ObservableCollection<DeviceRAMInfo>(await _infoService.GetDeviceRAMInfoAsync());
                DeviceDriveInfos = new ObservableCollection<DeviceDriveInfo>(await _infoService.GetDeviceDriveInfoAsync());
            }
            else
            {
                CurrentDevice = device;
                IsCurrentDevice = string.Equals(CurrentDevice.DeviceUUID, currentDevice.DeviceUUID, StringComparison.Ordinal);

                var deviceInfo = await _mainApi.GetDeviceInfoAsync(CurrentDevice.DeviceUUID);

                if (deviceInfo is null)
                {
                    return;
                }

                DeviceCPUInfos = new ObservableCollection<DeviceCPUInfo>(deviceInfo.CPUInfos);
                DeviceGPUInfos = new ObservableCollection<DeviceGPUInfo>(deviceInfo.GPUInfos);
                DeviceRAMInfos = new ObservableCollection<DeviceRAMInfo>(deviceInfo.RAMInfos);
                DeviceDriveInfos = new ObservableCollection<DeviceDriveInfo>(deviceInfo.DriveInfos);
            }
        }

        public DeviceViewModel(
            MainHub mainHub,
            MainApi mainApi,
            NavigationService navigationService,
            DeviceIdentityService identityService, 
            DeviceInfoService infoService,
            CommandExecutorService commandExecutorService
        )
        {
            _mainHub = mainHub;
            _mainApi = mainApi;
            _navigationService = navigationService;
            _identityService = identityService;
            _infoService = infoService;

            IsConnected = _mainHub.IsConnected;

            _mainHub.ConnectionStatusChanged += OnConnectionStatusChanged;
            _mainHub.DeviceInfoReceived += OnDeviceInfoReceived;
        }

        public DeviceViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _mainHub = null!;
            _mainApi = null!;
            _navigationService = null!;
            _identityService = new DeviceIdentityService();
            _infoService = new DeviceInfoService();

            IsCurrentDevice = true;

            CurrentDevice = new DeviceIdentity(
                DeviceName: "Крутое название",
                OperatingSystemType: OperatingSystemType.Windows,
                OperatingSystemName: "Windows какой то",
                DeviceManufacturer: "Крутой производитель"
            );

            DeviceCPUInfos = [
                new DeviceCPUInfo("AMD Ryzen", 6, 12)
            ];

            DeviceGPUInfos = [
                new DeviceGPUInfo("NVIDIA", 12L * 1024 * 1024 * 1024),
                new DeviceGPUInfo("AMD", 16L * 1024 * 1024 * 1024)
            ];

            DeviceRAMInfos = [
                new DeviceRAMInfo("ADATA", 8L * 1024 * 1024 * 1024, "DDR3", 1333),
                new DeviceRAMInfo("ADATA", 8L * 1024 * 1024 * 1024, "DDR3", 1333)
            ];

            DeviceDriveInfos = [
                new DeviceDriveInfo("Какой то диск HDD", 1024L * 1024 * 1024 * 1024)
            ];
        }

        [RelayCommand]
        public async Task NavigateToDeviceConsole()
        {
            await _navigationService.NavigateAsync<DeviceCommandConsoleViewModel, DeviceIdentity?>(CurrentDevice);
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsConnected = isConnected;
            });
        }

        private async void OnDeviceInfoReceived(DeviceInfoResponse response)
        {
            var currentDevice = await _identityService.GetCurrentDeviceAsync();
            if (!string.Equals(currentDevice.DeviceUUID, response.RequestorDeviceId, StringComparison.Ordinal))
                return;

            Dispatcher.UIThread.Post(() =>
            {
                DeviceCPUInfos = new ObservableCollection<DeviceCPUInfo>(response.CPUInfos);
                DeviceGPUInfos = new ObservableCollection<DeviceGPUInfo>(response.GPUInfos);
                DeviceRAMInfos = new ObservableCollection<DeviceRAMInfo>(response.RAMInfos);
                DeviceDriveInfos = new ObservableCollection<DeviceDriveInfo>(response.DriveInfos);
            });
        }

        public void Dispose()
        {
            _mainHub.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _mainHub.DeviceInfoReceived -= OnDeviceInfoReceived;
        }
    }
}
