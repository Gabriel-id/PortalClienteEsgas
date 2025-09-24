using Microsoft.IdentityModel.Tokens;
using PortalClienteAPI.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PortalClienteAPI.Services
{
    public class AuthenticateService
    {
        private readonly SapService _sapService;

        public AuthenticateService()
        {
            _sapService = new SapService();
        }

        public async Task<AuthResponse> Auth(AuthRequest authRequest)
        {
            var authResponse = await _sapService.Autenticate(authRequest);
            authResponse.token = GenerateToken(authResponse.clientCode);

            return authResponse;
        }

        public string GenerateToken(string clientCode)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("c2296ee5-453d-4e85-9680-b68ffbf5d99c");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("data", clientCode)
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
