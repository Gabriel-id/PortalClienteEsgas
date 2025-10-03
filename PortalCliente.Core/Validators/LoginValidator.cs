using FluentValidation;
using PortalCliente.Core.Dtos;

namespace PortalCliente.Core.Validators;

public class LoginValidator : AbstractValidator<Login>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("CPF ou CNPJ é obrigatório")
            .Must(BeValidCpfOrCnpj)
            .WithMessage("CPF deve ter 11 dígitos ou CNPJ deve ter 14 dígitos");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Código do cliente é obrigatório")
            .Length(1, 20)
            .WithMessage("Código do cliente deve ter entre 1 e 20 caracteres");
    }

    private static bool BeValidCpfOrCnpj(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // Remove non-numeric characters
        var cleanUsername = new string(username.Where(char.IsDigit).ToArray());

        // Check if it's a valid CPF (11 digits) or CNPJ (14 digits)
        return cleanUsername.Length == 11 || cleanUsername.Length == 14;
    }
}