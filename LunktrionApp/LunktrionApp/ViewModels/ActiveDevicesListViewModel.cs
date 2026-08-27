using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Hubs;
using LunktrionApp.Services;
using LunktrionShared.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class ActiveDevicesListViewModel : ViewModelBase, IDisposable
    {
        private readonly MainHub _mainHub;
        private readonly NavigationService _navigationService;

        public ObservableCollection<DeviceIdentity> Devices { get; } = [];

        public ActiveDevicesListViewModel(MainHub mainHub, NavigationService navigationService)
        {
            _mainHub = mainHub;
            _navigationService = navigationService;

            _mainHub.DeviceConnected += OnDeviceConnected;
        }

        public ActiveDevicesListViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _mainHub = null!;
            _navigationService = null!;

            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк", OperatingSystemName: "Windows OS"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон унопочный", OperatingSystemName: "Linux"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк 2", OperatingSystemName: "Windows OS 2"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон телепатический", OperatingSystemName: "Linux Windows"));
        }

        private async Task NavigateToDevice(DeviceIdentity device)
        {
            await _navigationService.NavigateAsync<DeviceViewModel, DeviceIdentity?>(device);
        }

        [RelayCommand]
        public async Task NavigateToDeviceCommandAsync(DeviceIdentity device)
        {
            await NavigateToDevice(device);
        }

        private void OnDeviceConnected(DeviceIdentity newDevice)
        {
            Dispatcher.UIThread.Post(() => Devices.Add(newDevice));
        }

        public void Dispose()
        {
            _mainHub.DeviceConnected -= OnDeviceConnected;
        }
    }
}
