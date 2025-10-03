using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Text.Json;
using PortalCliente.Core.Dtos;

namespace PortalCliente.Services;

public class MockSapServer : IDisposable
{
    private WireMockServer? _server;
    private readonly ILogger<MockSapServer> _logger;

    public MockSapServer(ILogger<MockSapServer> logger)
    {
        _logger = logger;
    }

    public void Start(int port = 8080)
    {
        try
        {
            _server = WireMockServer.Start(port);
            SetupEndpoints();
            _logger.LogInformation("MockSapServer started on port {Port}", port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MockSapServer on port {Port}", port);
            throw;
        }
    }

    private void SetupEndpoints()
    {
        SetupAuthenticationEndpoint();
        SetupGetInvoicesEndpoint();
        SetupGetInvoiceContentEndpoint();
    }

    private void SetupAuthenticationEndpoint()
    {
        _server!.Given(Request.Create()
            .WithPath("/sap/bc/inbound/DATAGAS003")
            .WithParam("sap-client", "600")
            .UsingPost())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(JsonSerializer.Serialize(new AuthResponse
            {
                ClientCode = "12345",
                ClientName = "Cliente Teste Mock",
                Token = "mock-token-123456789"
            })));

        _logger.LogInformation("Authentication endpoint configured");
    }

    private void SetupGetInvoicesEndpoint()
    {
        _server!.Given(Request.Create()
            .WithPath("/sap/bc/inbound/DATAGAS004")
            .WithParam("sap-client", "600")
            .WithHeader("token", "*")
            .UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(JsonSerializer.Serialize(new InvoicesData
            {
                Invoices = GetMockInvoices()
            })));

        _logger.LogInformation("GetInvoices endpoint configured");
    }

    private void SetupGetInvoiceContentEndpoint()
    {
        _server!.Given(Request.Create()
            .WithPath("/sap/bc/inbound/DATAGAS005")
            .WithParam("sap-client", "600")
            .WithHeader("token", "*")
            .UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(JsonSerializer.Serialize(GetMockInvoiceContent())));

        _logger.LogInformation("GetInvoiceContent endpoint configured");
    }

    private static IEnumerable<Invoice> GetMockInvoices()
    {
        return new List<Invoice>
        {
            new()
            {
                Document = "DOC001",
                InvoiceNumber = "INV001",
                BarcodeNumber = "123456789012345678901234567890123456789012345",
                CreatedAt = "2024-01-15",
                DueDate = "2024-02-15",
                Value = "150.75",
                Penalty = "0.00",
                Negative = "NAO",
                ClientNumber = "12345",
                Address = "Rua das Flores, 123 - Centro - São Paulo/SP",
                Status = "Em aberto",
                InvoiceStatus = "PENDENTE"
            },
            new()
            {
                Document = "DOC002",
                InvoiceNumber = "INV002",
                BarcodeNumber = "098765432109876543210987654321098765432109876",
                CreatedAt = "2024-02-15",
                DueDate = "2024-03-15",
                Value = "89.50",
                Penalty = "2.50",
                Negative = "NAO",
                ClientNumber = "12345",
                Address = "Rua das Flores, 123 - Centro - São Paulo/SP",
                Status = "Vencida",
                InvoiceStatus = "VENCIDA"
            },
            new()
            {
                Document = "DOC003",
                InvoiceNumber = "INV003",
                BarcodeNumber = "567890123456789012345678901234567890123456789",
                CreatedAt = "2024-03-15",
                DueDate = "2024-04-15",
                Value = "205.25",
                Penalty = "0.00",
                Negative = "NAO",
                ClientNumber = "12345",
                Address = "Rua das Flores, 123 - Centro - São Paulo/SP",
                Status = "Paga",
                InvoiceStatus = "PAGA"
            }
        };
    }

    private static Invoice GetMockInvoiceContent()
    {
        return new Invoice
        {
            Document = "DOC001",
            InvoiceNumber = "INV001",
            BarcodeNumber = "123456789012345678901234567890123456789012345",
            CreatedAt = "2024-01-15",
            DueDate = "2024-02-15",
            Value = "150.75",
            Penalty = "0.00",
            Negative = "NAO",
            ClientNumber = "12345",
            Address = "Rua das Flores, 123 - Centro - São Paulo/SP",
            Status = "Em aberto",
            InvoiceStatus = "PENDENTE",
            PdfContent = ""
        };
    }

    public void Stop()
    {
        _server?.Stop();
        _logger.LogInformation("MockSapServer stopped");
    }

    public void Dispose()
    {
        Stop();
        _server?.Dispose();
    }
}