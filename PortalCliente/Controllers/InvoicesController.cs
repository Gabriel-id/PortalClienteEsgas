using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Models;
using System.Diagnostics;

namespace PortalCliente.Controllers
{
    [Authorize]
    public class InvoicesController(IInvoicesService invoiceService) : BaseController
    {
        public async Task<IActionResult> Index()
        {

            var invoices = await invoiceService.GetInvoices(GetUser().ClientCode);

            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> Download(string invoiceNumber, string document)
        {
            var invoice = await invoiceService.GetInvoiceContent(GetUser().ClientCode, invoiceNumber, document);

            if (string.IsNullOrEmpty(invoice?.PdfContent))
            {
                SendToastMessage("Não foi possível baixar a fatura, por favor tente novamente mais tarde", "error");
                return NotFound();
            }

            var fileBytes = Convert.FromBase64String(invoice?.PdfContent!);
            var fileName = $"{invoiceNumber}_{document}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
