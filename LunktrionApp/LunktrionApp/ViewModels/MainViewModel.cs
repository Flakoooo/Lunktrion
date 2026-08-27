using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LunktrionApp.Hubs;
using LunktrionApp.Models.Interfaces;
using LunktrionApp.Services;
using System;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable, IAsyncInitializable
{
    private readonly MainHub _mainHub;
    private readonly DeviceIdentityService _deviceIdentityService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    public partial ViewModelBase? Navigation { get; set; }

    [ObservableProperty]
    public partial ViewModelBase? ActiveDevicesList { get; set; }

    public ViewModelBase? CurrentViewModel => _navigationService.CurrentViewModel;

    public async Task InitializeAsync()
    {
        var currentDevice = await _deviceIdentityService.GetCurrentDeviceAsync();

        await _mainHub.ConnectAsync(currentDevice);
    }

    public MainViewModel(
        MainHub mainHub,
        DeviceIdentityService deviceIdentityService,
        NavigationPanelViewModel navigationPanelViewModel,
        ActiveDevicesListViewModel activeDevicesListViewModel,
        NavigationService navigationService
    )
    {
        _mainHub = mainHub;
        _deviceIdentityService = deviceIdentityService;
        _navigationService = navigationService;

        Navigation = navigationPanelViewModel;
        ActiveDevicesList = activeDevicesListViewModel;

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
        _navigationService = null!;

        Navigation = new NavigationPanelViewModel();
        ActiveDevicesList = new ActiveDevicesListViewModel();
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
