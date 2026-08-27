using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Hubs;
using LunktrionApp.Models.Entities;
using LunktrionApp.Models.Enums;
using LunktrionApp.Models.Interfaces;
using LunktrionApp.Services;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class DeviceCommandConsoleViewModel : ViewModelBase, IDisposable, IAsyncInitializable<DeviceIdentity?>
    {
        private readonly DeviceService _deviceService;
        private readonly DeviceIdentityService _deviceIdentityService;
        private readonly MainHub _mainHub;

        public ObservableCollection<DeviceIdentity> Devices { get; set; } = [];

        public Dictionary<string, ObservableCollection<ConsoleLogItem>> Logs { get; } = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDeviceSelected))]
        [NotifyPropertyChangedFor(nameof(SelectedDeviceLogs))]
        public partial DeviceIdentity? SelectedDevice { get; set; }
        public bool IsDeviceSelected => SelectedDevice is not null;
        public ObservableCollection<ConsoleLogItem>? SelectedDeviceLogs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedDevice?.DeviceId)) 
                    return null;

                if (!Logs.TryGetValue(SelectedDevice.DeviceId, out var _))
                    Logs[SelectedDevice.DeviceId] = [];

                return Logs[SelectedDevice.DeviceId];
            }
        }

        [ObservableProperty]
        public partial string? CommandInput { get; set; }

        private void AddNewLog(string deviceId, string command, ConsoleMessageType logType)
        {
            if (!Logs.TryGetValue(deviceId, out var deviceLogs))
            {
                deviceLogs = new ObservableCollection<ConsoleLogItem>();
                Logs[deviceId] = deviceLogs;
            }

            deviceLogs.Add(new ConsoleLogItem(command, logType));

            if (deviceLogs.Count > 10)
                deviceLogs.RemoveAt(0);
        }

        public async Task InitializeAsync(DeviceIdentity? device = null)
        {
            var devices = await _deviceService.GetAllDevices();
            Devices = new ObservableCollection<DeviceIdentity>(devices);

            var currentDevice = await _deviceIdentityService.GetCurrentDeviceAsync();

            if (device is not null)
            {
                if (!string.Equals(currentDevice.DeviceId, device.DeviceId, StringComparison.Ordinal))
                {
                    SelectedDevice = device;
                }
            }
        }

        public DeviceCommandConsoleViewModel(
            DeviceService deviceService,
            DeviceIdentityService deviceIdentityService, 
            MainHub mainHub
        )
        {
            _deviceService = deviceService;
            _deviceIdentityService = deviceIdentityService;
            _mainHub = mainHub;

            _mainHub.CommandResultReceived += OnCommandResultReceived;
        }

        public DeviceCommandConsoleViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _deviceService = null!;
            _deviceIdentityService = null!;
            _mainHub = null!;

            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк", OperatingSystemName: "Windows OS", DeviceManufacturer: "MSI"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон унопочный", OperatingSystemName: "Linux", DeviceManufacturer: "MSI"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк 2", OperatingSystemName: "Windows OS 2", DeviceManufacturer: "ASUS"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон телепатический", OperatingSystemName: "Linux Windows", DeviceManufacturer: "IPHONE"));

            SelectedDevice = Devices[1];

            AddNewLog(string.Empty, "docker compose up -d --build", ConsoleMessageType.Command);

            AddNewLog(string.Empty, "иш че удумал", ConsoleMessageType.Result);

            _ = InitializeAsync();
        }

        [RelayCommand]
        public async Task SelectDevice(DeviceIdentity deviceIdentity)
        {
            Dispatcher.UIThread.Post(() =>
            {
                SelectedDevice = deviceIdentity;
            });
        }

        [RelayCommand]
        public async Task CreateCommand()
        {
            if (SelectedDevice is not null && !string.IsNullOrWhiteSpace(CommandInput))
            {
                var currentDevice = await _deviceIdentityService.GetCurrentDeviceAsync();

                var commandText = CommandInput;
                Dispatcher.UIThread.Post(() =>
                {
                    AddNewLog(SelectedDevice.DeviceId, CommandInput, ConsoleMessageType.Command);
                    CommandInput = null;
                });

                await _mainHub.ExecuteCommandAsync(
                    SelectedDevice.DeviceId, CommandInput, currentDevice.DeviceId
                );
            }
        }

        private async void OnCommandResultReceived(DeviceExecuteCommandResponse response)
        {
            var currentDevice = await _deviceIdentityService.GetCurrentDeviceAsync();

            if (!string.Equals(currentDevice.DeviceId, response.RequestorDeviceId, StringComparison.Ordinal))
                return;

            Dispatcher.UIThread.Post(() =>
            {
                if (!Logs.TryGetValue(response.TargetDeviceId, out var deviceLogs))
                {
                    deviceLogs = new ObservableCollection<ConsoleLogItem>();
                    Logs[response.TargetDeviceId] = deviceLogs;
                }

                deviceLogs.Add(new ConsoleLogItem(response.Command, ConsoleMessageType.Command));

                AddNewLog(response.TargetDeviceId, response.Output, ConsoleMessageType.Result);
            });
        }

        public void Dispose()
        {
            _mainHub.CommandResultReceived -= OnCommandResultReceived;
        }
    }
}
