using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalClienteAPI.Dtos;
using PortalClienteAPI.Services;

namespace PortalClienteAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost()]
        public async Task<AuthResponse> Auth([FromBody] AuthRequest authRequest)
        {
            var authResponse = await new AuthenticateService().Auth(authRequest);

            return authResponse;
        }
    }
}
