using LunktrionApi.Hubs;
using LunktrionApi.Models.Entities;
using Microsoft.AspNetCore.SignalR;

namespace LunktrionApi.Services
{
    public class MainService(IHubContext<MainHub> hubContext)
    {
        private readonly IHubContext<MainHub> _hubContext = hubContext;

        public async void GetAllActiveDevices(string sender)
        {
            await _hubContext.Clients.All.SendAsync("SendDeviceName", sender);
        }

        public async void RequestDeviceToGetInfo(string sender, string deviceName)
        {
            await _hubContext.Clients.All.SendAsync("SendDeviceInfo", sender, deviceName);
        }
    }
}
