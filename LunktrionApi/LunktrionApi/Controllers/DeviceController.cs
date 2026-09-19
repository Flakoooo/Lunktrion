using LunktrionApi.Services;
using LunktrionShared.Models.DTOs;
using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LunktrionApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeviceController(DeviceService deviceService) : ControllerBase
    {
        private readonly DeviceService _deviceService = deviceService;

        [HttpGet("online/{deviceId}")]
        public async Task<IActionResult> CheckDeviceOnlineStatus(
            [FromRoute] string deviceId
        )
        {
            var isOnline = _deviceService.GetDeviceOnlineStatus(deviceId);

            return isOnline ? Ok() : NotFound();
        }

        [HttpGet()]
        public async Task<ActionResult<IReadOnlyCollection<DeviceIdentity>>> GetAllDevices()
        {
            var devices = await _deviceService.GetAllDevicesAsync();

            return Ok(devices);
        }

        [HttpGet("{deviceId}")]
        public async Task<ActionResult<DeviceInfoDTO>> GetDeviceInfo(
            [FromRoute] string deviceId
        )
        {
            var info = await _deviceService.GetDeviceInfoAsync(deviceId);

            return Ok(info);
        }

        [HttpPost("verify/code")]
        public async Task<IActionResult> VerifyCode(
            [FromBody] VerifyCodeRequest request
        )
        {
            var isCorrect = _deviceService.VerifyCode(request.Code);

            return isCorrect ? Ok() : Forbid();
        }
    }
}
