using PortalCliente.Core.Dtos;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace PortalCliente.Infrastructure.Services;

public class SapService : ISapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SapService> _logger;
    private readonly SapServiceOptions _options;

    public SapService(HttpClient httpClient, ILogger<SapService> logger, IOptions<SapServiceOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        ConfigureBasicAuth(_options.Username, _options.Password);
    }

    public void ConfigureBasicAuth(string username, string password)
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<AuthResponse> Authenticate(AuthRequest authRequest)
    {
        try
        {
            _logger.LogInformation("Attempting SAP authentication for client: {ClientNumber}", authRequest.ClientNumber);

            var json = JsonSerializer.Serialize(authRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var endpoint = $"{_options.Endpoints.Authentication}?sap-client={_options.SapClient}";
            var response = await _httpClient.PostAsync(endpoint, content);

            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStreamAsync();
            var authResponse = await JsonSerializer.DeserializeAsync<AuthResponse>(responseData);

            _logger.LogInformation("SAP authentication successful for client: {ClientNumber}", authRequest.ClientNumber);
            return authResponse ?? new AuthResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SAP authentication failed for client: {ClientNumber}", authRequest.ClientNumber);
            throw;
        }
    }

    public async Task<IEnumerable<Invoice>?> GetInvoices(string token)
    {
        try
        {
            _logger.LogInformation("Fetching invoices for token: {Token}", token);

            _httpClient.DefaultRequestHeaders.Add("token", token);
            var endpoint = $"{_options.Endpoints.GetInvoices}?sap-client={_options.SapClient}";
            var response = await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStreamAsync();
            var invoicesData = await JsonSerializer.DeserializeAsync<InvoicesData>(responseData);

            _logger.LogInformation("Successfully fetched {Count} invoices", invoicesData.Invoices?.Count() ?? 0);
            return invoicesData.Invoices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch invoices for token: {Token}", token);
            throw;
        }
    }

    public async Task<Invoice?> GetInvoiceContent(string clientNumber, string invoiceNumber, string document)
    {
        try
        {
            _logger.LogInformation("Fetching invoice content for invoice: {InvoiceNumber}, document: {Document}",
                invoiceNumber, document);

            _httpClient.DefaultRequestHeaders.Add("token", clientNumber);

            var json = JsonSerializer.Serialize(new { document, clientNumber, invoiceNumber });
            var endpoint = $"{_options.Endpoints.GetInvoiceContent}?sap-client={_options.SapClient}";
            var request = new HttpRequestMessage
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                RequestUri = new Uri($"{_httpClient.BaseAddress}{endpoint}"),
                Method = HttpMethod.Get,
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStreamAsync();
            var invoiceContent = await JsonSerializer.DeserializeAsync<Invoice?>(responseData);

            _logger.LogInformation("Successfully fetched invoice content for invoice: {InvoiceNumber}", invoiceNumber);
            return invoiceContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch invoice content for invoice: {InvoiceNumber}, document: {Document}",
                invoiceNumber, document);
            throw;
        }
    }
}
