using PortalCliente.Core.Dtos;
using PortalCliente.Core.Interfaces.Services;

namespace PortalCliente.Core.Services
{
    public class AuthService(ISapService sapService) : IAuthService
    {
        public Task<AuthResponse> Authenticate(Login login)
        {
            // Remove non-numeric characters to check length
            var cleanUsername = new string(login.Username.Where(char.IsDigit).ToArray());
            var isCpf = cleanUsername.Length == 11;

            var authSapRequest = new AuthRequest
            {
                Cpf = isCpf ? login.Username : string.Empty,
                Cnpj = isCpf ? string.Empty : login.Username,
                ClientNumber = login.Password,
            };

            return sapService.Authenticate(authSapRequest);
        }
    }
}
