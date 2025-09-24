using FluentValidation;
using PortalCliente.Core.Dtos;

namespace PortalCliente.Core.Validators;

public class LoginValidator : AbstractValidator<Login>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .Must(BeValidCpfOrCnpj)
            .WithMessage("Username must be a valid CPF (11 digits) or CNPJ (14 digits)");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Client number is required")
            .Length(1, 20)
            .WithMessage("Client number must be between 1 and 20 characters");
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