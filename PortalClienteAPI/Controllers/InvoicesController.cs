using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalClienteAPI.Dtos;
using PortalClienteAPI.Services;

namespace PortalClienteAPI.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Invoice>?> Invoices()
        {
            var clientCode = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "data")!.Value;
            var invoices = await new SapService().GetInvoices(clientCode);

            return invoices;
        }
        
        [HttpGet("check-pdf")]
        public async Task<object> GetInvoiceContent([FromQuery] string clientNumber, [FromQuery] string invoiceNumber, [FromQuery] string document)
        {
            var clientCode = User.Claims.FirstOrDefault(c => c.Type == "data")!.Value;
            var invoiceContent = await new SapService().GetInvoiceContent(clientCode, clientNumber, invoiceNumber, document);

            return new { pdfContent = invoiceContent?.PdfContent};
        }
    }
}
