using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PortalCliente.Infrastructure.Configuration;
using PortalCliente.Core.Dtos;
using PortalCliente.Infrastructure.Services;

namespace PortalCliente.Tests.Infrastructure;

public class SapServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<ILogger<SapService>> _loggerMock;
    private readonly SapServiceOptions _options;
    private readonly HttpClient _httpClient;
    private readonly SapService _sapService;

    public SapServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _loggerMock = new Mock<ILogger<SapService>>();

        _options = new SapServiceOptions
        {
            BaseUrl = "http://test-server:8000/sap/bc/inbound/",
            Username = "TEST_USER",
            Password = "TEST_PASS",
            SapClient = "600",
            TimeoutSeconds = 30,
            Endpoints = new SapEndpoints
            {
                Authentication = "DATAGAS003",
                GetInvoices = "DATAGAS004",
                GetInvoiceContent = "DATAGAS005"
            }
        };

        var optionsMock = new Mock<IOptions<SapServiceOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };

        _sapService = new SapService(_httpClient, _loggerMock.Object, optionsMock.Object);
    }

    [Fact]
    public async Task Authenticate_Should_Return_AuthResponse_When_Successful()
    {
        var authRequest = new AuthRequest { ClientNumber = "12345", Cpf = "12345678901" };
        var expectedResponse = new AuthResponse { ClientCode = "12345", ClientName = "João Silva" };
        var responseJson = JsonSerializer.Serialize(expectedResponse);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _sapService.Authenticate(authRequest);

        result.Should().NotBeNull();
        result.ClientCode.Should().Be("12345");
        result.ClientName.Should().Be("João Silva");
    }

    [Fact]
    public async Task Authenticate_Should_Throw_Exception_When_Http_Error()
    {
        var authRequest = new AuthRequest { ClientNumber = "12345", Cpf = "12345678901" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var action = async () => await _sapService.Authenticate(authRequest);

        await action.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetInvoices_Should_Return_Invoices_List_When_Successful()
    {
        var invoicesData = new InvoicesData
        {
            Invoices = new[]
            {
                new Invoice
                {
                    Document = "001",
                    InvoiceNumber = "123456",
                    Value = "R$ 89,50",
                    DueDate = "2024-01-15",
                    Status = "Pendente",
                    BarcodeNumber = "12345678901234567890123456789012345678901234567890"
                }
            }
        };

        var responseJson = JsonSerializer.Serialize(invoicesData);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _sapService.GetInvoices("12345");

        result.Should().HaveCount(1);
        var invoiceList = result!.ToList();
        invoiceList.First().Document.Should().Be("001");
        invoiceList.First().InvoiceNumber.Should().Be("123456");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}