using PortalCliente.Core.Dtos;

namespace PortalCliente.Core.Interfaces.Services
{
    public interface ISapService
    {
        Task<AuthResponse> Authenticate(AuthRequest authRequest);
        Task<IEnumerable<Invoice>?> GetInvoices(string clientNumber);
        Task<Invoice?> GetInvoiceContent(string clientNumber, string invoiceNumber, string document);
    }
}
