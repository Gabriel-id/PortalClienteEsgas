using PortalCliente.Core.Dtos;
using PortalCliente.Core.Interfaces.Services;

namespace PortalCliente.Core.Services
{
    public class InvoicesService(ISapService sapService) : IInvoicesService
    {
        public Task<Invoice?> GetInvoiceContent(string clientNumber, string invoiceNumber, string document)
        {
            return sapService.GetInvoiceContent(clientNumber, invoiceNumber, document);
        }

        public Task<IEnumerable<Invoice>?> GetInvoices(string clientNumber)
        {
            return sapService.GetInvoices(clientNumber);
        }
    }
}
