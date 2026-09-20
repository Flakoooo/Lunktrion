using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Abstractions;
using LunktrionApp.Services;
using LunktrionShared.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class DevicesListViewModel : ViewModelBase, IAsyncInitializable<DeviceIdentity?>
    {
        private readonly DeviceIdentityService _deviceIdentityService;
        private readonly DeviceService _deviceService;

        public Action<DeviceIdentity>? DeviceSelected;

        public ObservableCollection<DeviceIdentity> Devices { get; set; } = [];

        [ObservableProperty]
        public partial DeviceIdentity? SelectedDevice { get; set; }

        public async Task InitializeAsync(DeviceIdentity? device = null)
        {
            var devices = await _deviceService.GetAllDevices();
            Devices = new ObservableCollection<DeviceIdentity>(devices);

            var currentDevice = await _deviceIdentityService.GetCurrentDeviceAsync();

            if (device is not null && !string.Equals(currentDevice.DeviceUUID, device.DeviceUUID, StringComparison.Ordinal))
            {
                SelectedDevice = device;
            }
        }

        public DevicesListViewModel(
            DeviceIdentityService deviceIdentityService,
            DeviceService deviceService
        )
        {
            _deviceIdentityService = deviceIdentityService;
            _deviceService = deviceService;
        }

        public DevicesListViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _deviceIdentityService = null!;
            _deviceService = null!;

            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк", OperatingSystemName: "Windows OS", DeviceManufacturer: "MSI"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон унопочный", OperatingSystemName: "Linux", DeviceManufacturer: "MSI"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк 2", OperatingSystemName: "Windows OS 2", DeviceManufacturer: "ASUS"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон телепатический", OperatingSystemName: "Linux Windows", DeviceManufacturer: "IPHONE"));

            SelectedDevice = Devices[1];
        }

        [RelayCommand]
        public async Task SelectDevice(DeviceIdentity deviceIdentity)
        {
            Dispatcher.UIThread.Post(() =>
            {
                SelectedDevice = deviceIdentity;
                DeviceSelected?.Invoke(SelectedDevice);
            });
        }
    }
}
