using Microsoft.AspNetCore.Mvc;
using System;

namespace RealtimeHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Realtime hub is running",
                timestamp = DateTime.UtcNow,
                service = "SignalR + RabbitMQ Consumer"
            });
        }
    }
}