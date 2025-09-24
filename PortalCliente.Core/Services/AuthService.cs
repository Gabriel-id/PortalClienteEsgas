using PortalCliente.Core.Dtos;
using PortalCliente.Core.Interfaces.Services;

namespace PortalCliente.Core.Services
{
    public class AuthService(ISapService sapService) : IAuthService
    {
        public Task<AuthResponse> Authenticate(Login login)
        {
            var isCpf = login.Username.Length <= 11;

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
