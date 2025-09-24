namespace PortalCliente.Core.Configuration;

public class SapServiceOptions
{
    public const string SectionName = "SapService";

    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SapClient { get; set; } = "600";
    public int TimeoutSeconds { get; set; } = 30;

    public SapEndpoints Endpoints { get; set; } = new();
}

public class SapEndpoints
{
    public string Authentication { get; set; } = "DATAGAS003";
    public string GetInvoices { get; set; } = "DATAGAS004";
    public string GetInvoiceContent { get; set; } = "DATAGAS005";
}