using System.ComponentModel.DataAnnotations;

namespace PortalCliente.Core.Dtos;

public class Login
{
    [Display(Name = "CPF ou CNPJ")]
    [Required(ErrorMessage = "O campo CPF ou CNPJ é obrigatório.")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Código do Cliente")]
    [Required(ErrorMessage = "O campo Código do Cliente é obrigatório.")]
    public string Password { get; set; } = string.Empty;
}
