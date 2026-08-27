using LunktrionApp.Api;
using LunktrionApp.Hubs;
using LunktrionApp.Models.Interfaces;
using LunktrionApp.Services;
using LunktrionApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LunktrionApp
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {
            // API
            collection.AddHttpClient<MainApi>(client =>
            {
                client.BaseAddress = new Uri($"{BuildConfig.ApiBaseUrl}/api/");
            });
            collection.AddSingleton<MainHub>();

            // Services
            collection.AddSingleton<NavigationService>();
            collection.AddSingleton<IAsyncInitializable>(
                sp => sp.GetRequiredService<NavigationService>());

            collection.AddSingleton<HardwareService>();
            collection.AddSingleton<DeviceService>();
            collection.AddSingleton<DeviceIdentityService>();
            collection.AddSingleton<DeviceInfoService>();

            collection.AddSingleton<CommandExecutorService>();

            // ViewModels
            collection.AddSingleton<LoadingViewModel>();
            collection.AddSingleton<ActiveDevicesListViewModel>();
            collection.AddSingleton<NavigationPanelViewModel>();

            collection.AddSingleton<MainViewModel>();
            collection.AddSingleton<IAsyncInitializable>(sp => sp.GetRequiredService<MainViewModel>());

            collection.AddTransient<DeviceViewModel>();
            collection.AddTransient<DevicesListViewModel>();
            collection.AddTransient<DeviceCommandConsoleViewModel>();
        }

        public static async Task InitializeAsync(this IServiceProvider provider)
        {
            foreach (var service in provider.GetServices<IAsyncInitializable>())
            {
                await service.InitializeAsync();
            }
        }
    }
}
