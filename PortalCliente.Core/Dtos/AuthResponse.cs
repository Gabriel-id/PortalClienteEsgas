using System.Text.Json.Serialization;

namespace PortalCliente.Core.Dtos;

public class AuthResponse
{
    [JsonPropertyName("clientCode")]
    public string ClientCode { get; set; } = string.Empty;

    [JsonPropertyName("clientName")]
    public string ClientName { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
