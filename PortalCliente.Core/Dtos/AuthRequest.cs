namespace PortalCliente.Core.Dtos;

public class AuthRequest
{
    public string ClientNumber { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
}
