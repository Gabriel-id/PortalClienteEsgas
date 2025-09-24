using System.Text.Json.Serialization;

namespace PortalCliente.Core.Dtos;

public class Invoice
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("invoiceStatus")]
    public string InvoiceStatus { get; set; } = string.Empty;

    [JsonPropertyName("pdfContent")]
    public string PdfContent { get; set; } = string.Empty;

    [JsonPropertyName("document")]
    public string Document { get; set; } = string.Empty;

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonPropertyName("barcodeNumber")]
    public string BarcodeNumber { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("duedate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("penalty")]
    public string Penalty { get; set; } = string.Empty;

    [JsonPropertyName("negativo")]
    public string Negative { get; set; } = string.Empty;

    [JsonPropertyName("clientnumber")]
    public string ClientNumber { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id => Document;

    public string IsNegative => Negative == "NAO" ? "Não" : "Sim";
}

public class InvoicesData
{
    [JsonPropertyName("invoices")]
    public IEnumerable<Invoice> Invoices { get; set; } = new List<Invoice>();
}
