using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LunktrionApp.Abstractions;
using LunktrionApp.Services;
using LunktrionShared.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class FileExplorerViewModel : ViewModelBase, IDisposable, IAsyncInitializable<DeviceIdentity?>
    {
        private readonly DeviceIdentityService _deviceIdentityService;

        public DevicesListViewModel DevicesListViewModel { get; set; }

        public ObservableCollection<DeviceIdentity> Devices { get; set; } = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDeviceSelected))]
        public partial DeviceIdentity? SelectedDevice { get; set; }
        public bool IsDeviceSelected => SelectedDevice is not null;

        public async Task InitializeAsync(DeviceIdentity? device = null)
        {
            await DevicesListViewModel.InitializeAsync(device);

            Devices = DevicesListViewModel.Devices;
        }

        public FileExplorerViewModel(
            DeviceIdentityService deviceIdentityService,
            DeviceService deviceService,
            DevicesListViewModel devicesListViewModel
        )
        {
            _deviceIdentityService = deviceIdentityService;
            DevicesListViewModel = devicesListViewModel;

            DevicesListViewModel.DeviceSelected += OnDeviceSelected;
        }

        public FileExplorerViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _deviceIdentityService = null!;
            DevicesListViewModel = new DevicesListViewModel();
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
            DevicesListViewModel.DeviceSelected -= OnDeviceSelected;
        }
    }
}
