using LunktrionApp.Api;
using LunktrionShared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunktrionApp.Services
{
    public class DeviceService(MainApi mainApi)
    {
        private readonly MainApi _mainApi = mainApi;

        private List<DeviceIdentity> _devices = [];
        private DateTime _lastRefreshTime = DateTime.MinValue;

        public async Task<List<DeviceIdentity>> GetAllDevices()
        {
            if (_lastRefreshTime.AddMinutes(1) < DateTime.Now)
            {
                _devices = (await _mainApi.GetAllDevices()).ToList();
                _lastRefreshTime = DateTime.Now;
            }

            return _devices;
        }

    }
}
