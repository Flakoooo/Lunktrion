using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Abstractions;
using LunktrionApp.Api;
using LunktrionApp.Hubs;
using LunktrionApp.Modals.Parameters;
using LunktrionApp.Modals.Results;
using LunktrionApp.Services;
using LunktrionApp.Services.CommandExecutors;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Enums;
using LunktrionShared.Models.Responses;
using LunktrionShared.Utils;
using System;
using System.Collections.ObjectModel;
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
        private readonly ModalService _modalService;
        private readonly NotificationService _notificationService;
        private readonly ICommandExecutor _commandExecutor;

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

            CurrentDevice = device ?? currentDevice;
            IsCurrentDevice = string.Equals(CurrentDevice.DeviceUUID, currentDevice.DeviceUUID, StringComparison.Ordinal);

            if (IsCurrentDevice)
            {
                _mainHub.ConnectionStatusChanged += OnConnectionStatusChanged;

                IsConnected = _mainHub.IsConnected;

                DeviceCPUInfos = new ObservableCollection<DeviceCPUInfo>(await _infoService.GetDeviceCPUInfoAsync());
                DeviceGPUInfos = new ObservableCollection<DeviceGPUInfo>(await _infoService.GetDeviceGPUInfoAsync());
                DeviceRAMInfos = new ObservableCollection<DeviceRAMInfo>(await _infoService.GetDeviceRAMInfoAsync());
                DeviceDriveInfos = new ObservableCollection<DeviceDriveInfo>(await _infoService.GetDeviceDriveInfoAsync());
            }
            else
            {
                _mainHub.DeviceDisconnected += OnDeviceDisconnected;
                _mainHub.DeviceInfoReceived += OnDeviceInfoReceived;

                IsConnected = await _mainApi.CheckDeviceOnlineStatusAsync(CurrentDevice.DeviceUUID);

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
            ModalService modalService,
            NotificationService notificationService,
            ICommandExecutor commandExecutor
        )
        {
            _mainHub = mainHub;
            _mainApi = mainApi;
            _navigationService = navigationService;
            _identityService = identityService;
            _infoService = infoService;
            _modalService = modalService;
            _notificationService = notificationService;
            _commandExecutor = commandExecutor;
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
            _modalService = null!;
            _notificationService = null!;
            _commandExecutor = null!;

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
        public async Task UpdateDeviceInfo()
        {
            if (CurrentDevice is null) return;

            var currentDevice = await _identityService.GetCurrentDeviceAsync();
            if (string.Equals(currentDevice.DeviceUUID, CurrentDevice.DeviceUUID, StringComparison.Ordinal))
                return;

            await _mainHub.UpdateDeviceInfoAsync(CurrentDevice.DeviceUUID, currentDevice.DeviceUUID);
        }

        [RelayCommand]
        public async Task NavigateToDeviceConsole()
        {
            await _navigationService.NavigateAsync<DeviceCommandConsoleViewModel, DeviceIdentity?>(CurrentDevice);
        }

        [RelayCommand]
        public async Task ShutdownDevice()
        {
            if (CurrentDevice is null) return;

            var shutdownResult = await _modalService.ShowModalAsync<ConfirmModalViewModel, ConfirmModalParams, ConfirmResult>(
                new ConfirmModalParams(
                    $"Вы уверены что хотите выключить {CurrentDevice.DeviceName}?",
                    ConfirmType.Delete,
                    CodeInputRequired: !IsCurrentDevice,
                    DisplayCodeText: "Введите код подтверждения для экстренного выключения",
                    SuccessText: "Выключить"
                )
            );

            if (!shutdownResult.IsConfirmed) return;

            if (IsCurrentDevice)
            {
                string shutdownCommand = CommandHandler.ShutdownCommand(OperatingSystemType.Windows, 0);
                if (string.IsNullOrWhiteSpace(shutdownCommand))
                {
                    _notificationService.ShowError("Команда выключения не найдена");
                    return;
                }

                var executorResult = await _commandExecutor.ExecuteCommandAsync(shutdownCommand);
                if (!executorResult.IsSuccess)
                {
                    _notificationService.ShowError("Не удалось выключить устройство");
                    return;
                }
            }
            else
            {
                if (!shutdownResult.Code.HasValue) return;

                var currentDevice = await _identityService.GetCurrentDeviceAsync();

                await _mainHub.ShutdownDeviceAsync(
                    CurrentDevice.DeviceUUID, currentDevice.DeviceUUID, shutdownResult.Code.Value
                );
            }
        }

        /// <summary>
        /// Метод для изменения статуса в сети для ТЕКУЩЕГО устройства
        /// </summary>
        /// <param name="isConnected"></param>
        private void OnConnectionStatusChanged(bool isConnected)
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsConnected = isConnected;
            });
        }


        /// <summary>
        /// Метод для переключения статуса в сети для КОНКРЕТНОГО устройства
        /// </summary>
        /// <param name="deviceId"></param>
        private void OnDeviceDisconnected(string deviceId)
        {
            if (CurrentDevice is null
                || !string.Equals(CurrentDevice.DeviceUUID, deviceId, StringComparison.Ordinal)
            ) return;

            Dispatcher.UIThread.Post(() =>
            {
                IsConnected = false;
            });
        }

        private async void OnDeviceInfoReceived(DeviceInfoResponse response)
        {
            if (CurrentDevice is null 
                || !string.Equals(CurrentDevice.DeviceUUID, response.TargetDeviceId, StringComparison.Ordinal)
            ) return;

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
            if (IsCurrentDevice)
            {
                _mainHub.ConnectionStatusChanged -= OnConnectionStatusChanged;
            }
            else
            {
                _mainHub.DeviceDisconnected -= OnDeviceDisconnected;
                _mainHub.DeviceInfoReceived -= OnDeviceInfoReceived;
            }
        }
    }
}
