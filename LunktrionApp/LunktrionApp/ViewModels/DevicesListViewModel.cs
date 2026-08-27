using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Models.Interfaces;
using LunktrionApp.Services;
using LunktrionShared.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class DevicesListViewModel : ViewModelBase, IAsyncInitializable
    {
        private readonly DeviceService _deviceService;
        private readonly NavigationService _navigationService;

        public ObservableCollection<DeviceIdentity> Devices { get; set; } = [];

        public async Task InitializeAsync()
        {
            var devices = await _deviceService.GetAllDevices();
            Devices = new ObservableCollection<DeviceIdentity>(devices);
        }

        public DevicesListViewModel(
            DeviceService deviceService,
            NavigationService navigationService
        )
        {
            _deviceService = deviceService;
            _navigationService = navigationService;
        }

        public DevicesListViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _deviceService = null!;
            _navigationService = null!;

            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк", OperatingSystemName: "Windows OS", DeviceManufacturer: "MSI"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон унопочный", OperatingSystemName: "Linux", DeviceManufacturer: "MSI"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк 2", OperatingSystemName: "Windows OS 2", DeviceManufacturer: "ASUS"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон телепатический", OperatingSystemName: "Linux Windows", DeviceManufacturer: "IPHONE"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк", OperatingSystemName: "Windows OS", DeviceManufacturer: "ACER"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон унопочный", OperatingSystemName: "Linux"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк 2", OperatingSystemName: "Windows OS 2"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон телепатический", OperatingSystemName: "Linux Windows"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк", OperatingSystemName: "Windows OS"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон унопочный", OperatingSystemName: "Linux"));
            Devices.Add(new DeviceIdentity(DeviceName: "Крутой пк 2", OperatingSystemName: "Windows OS 2"));
            Devices.Add(new DeviceIdentity(DeviceName: "Телефон телепатический", OperatingSystemName: "Linux Windows"));
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
    }
}
