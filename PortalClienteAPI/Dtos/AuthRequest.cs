namespace PortalClienteAPI.Dtos
{
    public struct AuthRequest
    {
        public string ClientNumber { get; set; }
        public string Cpf { get; set; }
        public string Cnpj { get; set; }
    }
}
