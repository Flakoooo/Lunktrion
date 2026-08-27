using LunktrionApp.Models.Entities;
using LunktrionApp.Models.Interfaces;
using LunktrionApp.ViewModels;
using LunktrionShared.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LunktrionApp.Services
{
    public class NavigationService : IAsyncInitializable
    {
        private readonly IServiceProvider _provider;
        private bool _isNavigating;

        public event Action? CurrentViewModelChanged;

        public NavigationService(IServiceProvider provider)
        {
            _provider = provider;

            CurrentViewModel = _provider.GetRequiredService<LoadingViewModel>();
        }

        public NavigationService()
        {
            _provider = null!;

            CurrentViewModel = new DeviceViewModel();
        }

        private ViewModelBase? _currentViewModel;

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel == value) return;

                _currentViewModel = value;

                CurrentViewModelChanged?.Invoke();
            }
        }

        public async Task InitializeAsync()
        {
            await NavigateAsync<DeviceViewModel, DeviceIdentity?>(null);
        }

        public async Task NavigateAsync<TViewModel>() where TViewModel : ViewModelBase
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                var oldViewModel = CurrentViewModel;
                var newViewModel = _provider.GetRequiredService<TViewModel>();

                CurrentViewModel = _provider.GetRequiredService<LoadingViewModel>();

                if (newViewModel is IAsyncInitializable initializable)
                    await initializable.InitializeAsync();

                if (oldViewModel is IDisposable disposable)
                    disposable.Dispose();

                CurrentViewModel = newViewModel;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        public async Task NavigateAsync<TViewModel, TParameter>(TParameter parameter) where TViewModel : ViewModelBase
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                var oldViewModel = CurrentViewModel;
                var newViewModel = _provider.GetRequiredService<TViewModel>();

                CurrentViewModel = _provider.GetRequiredService<LoadingViewModel>();

                if (newViewModel is IAsyncInitializable<TParameter> initializable)
                    await initializable.InitializeAsync(parameter);

                if (oldViewModel is IDisposable disposable)
                    disposable.Dispose();

                CurrentViewModel = newViewModel;
            }
            finally
            {
                _isNavigating = false;
            }
        }
    }
}
