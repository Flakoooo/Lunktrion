using LunktrionApi.Services;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LunktrionApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeviceController(DeviceService deviceService) : ControllerBase
    {
        private readonly DeviceService _deviceService = deviceService;

        [HttpGet()]
        public async Task<ActionResult<IReadOnlyCollection<DeviceIdentity>>> GetAllDevicesAsync()
        {
            var devices = await _deviceService.GetAllDevicesAsync();

            return Ok(devices);
        }

        [HttpGet("{deviceId}")]
        public async Task<ActionResult<DeviceInfoDTO>> GetDeviceInfoAsync([FromRoute] string deviceId)
        {
            var info = await _deviceService.GetDeviceInfoAsync(deviceId);

            return Ok(info);
        }
    }
}
