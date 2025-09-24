using PortalClienteAPI.Dtos;
using Refit;

namespace PortalClienteAPI.Services.Interface.Services
{
    public interface ISapService
    {
        [Post("/DATAGAS003?sap-client=600")]
        Task<AuthResponse> Autenticate(AuthRequest request);
    }
}
