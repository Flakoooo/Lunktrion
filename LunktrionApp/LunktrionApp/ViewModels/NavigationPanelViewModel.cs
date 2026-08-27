using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Models.Entities;
using LunktrionApp.Services;
using LunktrionShared.Models.Entities;
using System;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class NavigationPanelViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;

        public NavigationPanelViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public NavigationPanelViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _navigationService = new NavigationService();
        }


        [RelayCommand]
        public async Task NavigateToCurrentDeviceCommandAsync()
        {
            await _navigationService.NavigateAsync<DeviceViewModel, DeviceIdentity?>(null);
        }

        [RelayCommand]
        public async Task NavigateToAllDevicesCommandAsync()
        {
            await _navigationService.NavigateAsync<DevicesListViewModel>();
        }

        [RelayCommand]
        public async Task NavigateToCommandConsoleAsync()
        {
            await _navigationService.NavigateAsync<DeviceCommandConsoleViewModel, DeviceIdentity?>(null);
        }
    }
}
