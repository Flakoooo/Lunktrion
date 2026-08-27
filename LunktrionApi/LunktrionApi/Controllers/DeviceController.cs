using LunktrionApi.Services;
using LunktrionShared.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LunktrionApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeviceController(DeviceRegistry deviceRegistry) : ControllerBase
    {
        private readonly DeviceRegistry _deviceRegistry = deviceRegistry;

        [HttpGet()]
        public async Task<ActionResult<IReadOnlyCollection<DeviceIdentity>>> GetAllDevices()
        {
            var devices = _deviceRegistry.GetAllDevices();

            return Ok(devices);
        }
    }
}
