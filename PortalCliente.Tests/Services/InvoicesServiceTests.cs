using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalCliente.Core.Dtos;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Core.Services;

namespace PortalCliente.Tests.Services;

public class InvoicesServiceTests
{
    private readonly Mock<ISapService> _sapServiceMock;
    private readonly InvoicesService _invoicesService;

    public InvoicesServiceTests()
    {
        _sapServiceMock = new Mock<ISapService>();
        _invoicesService = new InvoicesService(_sapServiceMock.Object);
    }

    [Fact]
    public async Task GetInvoices_Should_Return_Invoices_List()
    {
        var clientCode = "12345";
        var expectedInvoices = new List<Invoice>
        {
            new()
            {
                Document = "001",
                InvoiceNumber = "123456",
                Value = "R$ 89,50",
                DueDate = "2024-01-15",
                Status = "Pendente",
                BarcodeNumber = "12345678901234567890123456789012345678901234567890"
            },
            new()
            {
                Document = "002",
                InvoiceNumber = "123457",
                Value = "R$ 95,30",
                DueDate = "2024-02-15",
                Status = "Pago",
                BarcodeNumber = "12345678901234567890123456789012345678901234567891"
            }
        };

        _sapServiceMock
            .Setup(x => x.GetInvoices(clientCode))
            .ReturnsAsync(expectedInvoices);

        var result = await _invoicesService.GetInvoices(clientCode);

        result.Should().HaveCount(2);
        var invoiceList = result!.ToList();
        invoiceList.First().InvoiceNumber.Should().Be("123456");
        invoiceList.Last().InvoiceNumber.Should().Be("123457");

        _sapServiceMock.Verify(x => x.GetInvoices(clientCode), Times.Once);
    }

    [Fact]
    public async Task GetInvoices_Should_Return_Empty_When_SAP_Returns_Null()
    {
        var clientCode = "12345";

        _sapServiceMock
            .Setup(x => x.GetInvoices(clientCode))
            .ReturnsAsync((IEnumerable<Invoice>?)null);

        var result = await _invoicesService.GetInvoices(clientCode);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInvoiceContent_Should_Return_Invoice()
    {
        var clientCode = "12345";
        var invoiceNumber = "123456";
        var document = "001";
        var expectedInvoice = new Invoice
        {
            Document = document,
            InvoiceNumber = invoiceNumber,
            Value = "R$ 89,50"
        };

        _sapServiceMock
            .Setup(x => x.GetInvoiceContent(clientCode, invoiceNumber, document))
            .ReturnsAsync(expectedInvoice);

        var result = await _invoicesService.GetInvoiceContent(clientCode, invoiceNumber, document);

        result.Should().NotBeNull();
        result!.Document.Should().Be(document);
        result.InvoiceNumber.Should().Be(invoiceNumber);

        _sapServiceMock.Verify(x => x.GetInvoiceContent(clientCode, invoiceNumber, document), Times.Once);
    }

    [Fact]
    public async Task GetInvoiceContent_Should_Return_Null_When_SAP_Returns_Null()
    {
        var clientCode = "12345";
        var invoiceNumber = "123456";
        var document = "001";

        _sapServiceMock
            .Setup(x => x.GetInvoiceContent(clientCode, invoiceNumber, document))
            .ReturnsAsync((Invoice?)null);

        var result = await _invoicesService.GetInvoiceContent(clientCode, invoiceNumber, document);

        result.Should().BeNull();
    }
}