using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace PortalClienteAPI.Dtos
{
    public struct Invoice
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("invoiceStatus")]
        public string InvoiceStatus { get; set; }

        [JsonPropertyName("pdfContent")]
        public string PdfContent { get; set; }

        [JsonPropertyName("document")]
        public string Document { get; set; }

        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonPropertyName("barcodeNumber")]
        public string BarcodeNumber { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("duedate")]
        public string DueDate { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("penalty")]
        public string Penalty { get; set; }

        [JsonPropertyName("negativo")]
        public string Negative { get; set; }

        [JsonPropertyName("clientnumber")]
        public string ClientNumber { get; set; }

        [JsonPropertyName("id")]
        public readonly string Id => Document;
    }

    public struct InvoicesData
    {
        [JsonPropertyName("invoices")]
        public IEnumerable<Invoice> Invoices { get; set; }
    }
}
