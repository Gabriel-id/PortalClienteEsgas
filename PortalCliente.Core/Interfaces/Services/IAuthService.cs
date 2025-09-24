using PortalCliente.Core.Dtos;

namespace PortalCliente.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Authenticate(Login login);
    }
}
