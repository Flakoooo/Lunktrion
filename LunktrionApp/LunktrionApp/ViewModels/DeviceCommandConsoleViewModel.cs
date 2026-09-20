using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Abstractions;
using LunktrionApp.Hubs;
using LunktrionApp.Models.Entities;
using LunktrionApp.Models.Enums;
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
        private readonly DeviceIdentityService _deviceIdentityService;
        private readonly MainHub _mainHub;

        public DevicesListViewModel DevicesListViewModel { get; set; }

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
                if (string.IsNullOrWhiteSpace(SelectedDevice?.DeviceUUID)) 
                    return null;

                if (!Logs.TryGetValue(SelectedDevice.DeviceUUID, out var _))
                    Logs[SelectedDevice.DeviceUUID] = [];

                return Logs[SelectedDevice.DeviceUUID];
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
            await DevicesListViewModel.InitializeAsync(device);

            Devices = DevicesListViewModel.Devices;
        }

        public DeviceCommandConsoleViewModel(
            DeviceIdentityService deviceIdentityService, 
            MainHub mainHub,
            DevicesListViewModel devicesListViewModel
        )
        {
            _deviceIdentityService = deviceIdentityService;
            _mainHub = mainHub;
            DevicesListViewModel = devicesListViewModel;

            _mainHub.CommandResultReceived += OnCommandResultReceived;
            DevicesListViewModel.DeviceSelected += OnDeviceSelected;
        }

        public DeviceCommandConsoleViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _deviceIdentityService = null!;
            _mainHub = null!;

            DevicesListViewModel = new DevicesListViewModel();

            AddNewLog(string.Empty, "docker compose up -d --build", ConsoleMessageType.Command);

            AddNewLog(string.Empty, "иш че удумал", ConsoleMessageType.Result);

            _ = InitializeAsync();
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
                    AddNewLog(SelectedDevice.DeviceUUID, CommandInput, ConsoleMessageType.Command);
                    CommandInput = null;
                });

                await _mainHub.ExecuteCommandAsync(
                    SelectedDevice.DeviceUUID, CommandInput, currentDevice.DeviceUUID
                );
            }
        }

        private async void OnCommandResultReceived(DeviceExecuteCommandResponse response)
        {
            var currentDevice = await _deviceIdentityService.GetCurrentDeviceAsync();

            if (!string.Equals(currentDevice.DeviceUUID, response.RequestorDeviceId, StringComparison.Ordinal))
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

        private void OnDeviceSelected(DeviceIdentity deviceIdentity)
        {
            Dispatcher.UIThread.Post(() =>
            {
                SelectedDevice = deviceIdentity;
            });
        }

        public void Dispose()
        {
            _mainHub.CommandResultReceived -= OnCommandResultReceived;
            DevicesListViewModel.DeviceSelected -= OnDeviceSelected;
        }
    }
}
