using Microsoft.AspNetCore.Mvc;

namespace PortalClienteAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Get()
        {
            return Ok(new {success = true});
        }
    }
}
