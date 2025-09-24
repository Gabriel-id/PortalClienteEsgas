using PortalCliente.Core.Dtos;

namespace PortalCliente.Core.Interfaces.Services
{
    public interface IInvoicesService
    {
        Task<IEnumerable<Invoice>?> GetInvoices(string clientNumber);
        Task<Invoice?> GetInvoiceContent(string clientNumber, string invoiceNumber, string document);
    }
}
