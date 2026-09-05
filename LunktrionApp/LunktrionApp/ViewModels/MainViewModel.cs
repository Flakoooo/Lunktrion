using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LunktrionApp.Hubs;
using LunktrionApp.Models.Interfaces;
using LunktrionApp.Services;
using LunktrionShared.Models.Requests;
using System;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable, IAsyncInitializable
{
    private readonly MainHub _mainHub;
    private readonly DeviceIdentityService _deviceIdentityService;
    private readonly DeviceInfoService _deviceInfoService;
    private readonly NavigationService _navigationService;

    public ViewModelBase NotificationViewModel { get; set; }

    [ObservableProperty]
    public partial ViewModelBase? Navigation { get; set; }

    [ObservableProperty]
    public partial ViewModelBase? ActiveDevicesList { get; set; }

    public ViewModelBase? CurrentViewModel => _navigationService.CurrentViewModel;

    public async Task InitializeAsync()
    {
        var currentDevice = await _deviceIdentityService.GetCurrentDeviceAsync();

        var cpuInfo = await _deviceInfoService.GetDeviceCPUInfoAsync();
        var gpuInfo = await _deviceInfoService.GetDeviceGPUInfoAsync();
        var ramInfo = await _deviceInfoService.GetDeviceRAMInfoAsync();
        var driveInfo = await _deviceInfoService.GetDeviceDriveInfoAsync();

        await _mainHub.ConnectAsync(
            new RegisterDeviceReuest(currentDevice, cpuInfo, gpuInfo, ramInfo, driveInfo)
        );
    }

    public MainViewModel(
        MainHub mainHub,
        DeviceIdentityService deviceIdentityService,
        DeviceInfoService deviceInfoService,
        NavigationPanelViewModel navigationPanelViewModel,
        ActiveDevicesListViewModel activeDevicesListViewModel,
        NavigationService navigationService,
        NotificationViewModel notificationViewModel
    )
    {
        _mainHub = mainHub;
        _deviceIdentityService = deviceIdentityService;
        _deviceInfoService = deviceInfoService;
        _navigationService = navigationService;

        Navigation = navigationPanelViewModel;
        ActiveDevicesList = activeDevicesListViewModel;
        NotificationViewModel = notificationViewModel;

        _navigationService.CurrentViewModelChanged += ChangeCurrentPage;
    }

    public MainViewModel()
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException(
                "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
            );
        }

        _mainHub = null!;
        _deviceIdentityService = null!;
        _deviceInfoService = null!;
        _navigationService = null!;

        Navigation = new NavigationPanelViewModel();
        ActiveDevicesList = new ActiveDevicesListViewModel();
        NotificationViewModel = new NotificationViewModel();
    }

    private void ChangeCurrentPage()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }


    public void Dispose()
    {
        _navigationService.CurrentViewModelChanged -= ChangeCurrentPage;
    }
}
