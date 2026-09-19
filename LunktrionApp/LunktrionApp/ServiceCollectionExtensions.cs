using LunktrionApp.Abstractions;
using LunktrionApp.Api;
using LunktrionApp.Hubs;
using LunktrionApp.Services;
using LunktrionApp.Services.CommandExecutors;
using LunktrionApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LunktrionApp
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {
            // API
            collection.AddHttpClient();
            collection.AddSingleton<MainHub>();
            collection.AddSingleton<MainApi>();

            // Services
            collection.AddSingleton<NotificationService>();
            collection.AddSingleton<ModalService>();
            collection.AddSingleton<NavigationService>();
            collection.AddSingleton<IAsyncInitializable>(
                sp => sp.GetRequiredService<NavigationService>());

            collection.AddSingleton<HardwareService>();
            collection.AddSingleton<DeviceService>();
            collection.AddSingleton<DeviceIdentityService>();
            collection.AddSingleton<DeviceInfoService>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                collection.AddSingleton<ICommandExecutor, WindowsCommandExecutor>();
            }
            else
            {
                //TODOO: заменить потом на реализацию под другие OS
                throw new ApplicationException("В данный момент данная операционная система не поддерживается");
            }


            collection.AddSingleton<CommandExecutorService>();

            // Modals
            collection.AddTransient<ConfirmModalViewModel>();

            // ViewModels
            collection.AddSingleton<LoadingViewModel>();
            collection.AddSingleton<ActiveDevicesListViewModel>();
            collection.AddSingleton<NavigationPanelViewModel>();
            collection.AddSingleton<NotificationViewModel>();
            collection.AddSingleton<ModalContainerViewModel>();

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
