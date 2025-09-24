using FluentValidation;
using PortalCliente.Core.Dtos;

namespace PortalCliente.Core.Validators;

public class AuthRequestValidator : AbstractValidator<AuthRequest>
{
    public AuthRequestValidator()
    {
        RuleFor(x => x.ClientNumber)
            .NotEmpty()
            .WithMessage("Client number is required")
            .Length(1, 20)
            .WithMessage("Client number must be between 1 and 20 characters");

        RuleFor(x => x)
            .Must(HaveValidCpfOrCnpj)
            .WithMessage("Either CPF or CNPJ must be provided");

        When(x => !string.IsNullOrEmpty(x.Cpf), () =>
        {
            RuleFor(x => x.Cpf)
                .Must(BeValidCpf)
                .WithMessage("CPF must have 11 digits");
        });

        When(x => !string.IsNullOrEmpty(x.Cnpj), () =>
        {
            RuleFor(x => x.Cnpj)
                .Must(BeValidCnpj)
                .WithMessage("CNPJ must have 14 digits");
        });
    }

    private static bool HaveValidCpfOrCnpj(AuthRequest request)
    {
        return !string.IsNullOrEmpty(request.Cpf) || !string.IsNullOrEmpty(request.Cnpj);
    }

    private static bool BeValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var cleanCpf = new string(cpf.Where(char.IsDigit).ToArray());
        return cleanCpf.Length == 11;
    }

    private static bool BeValidCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        var cleanCnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        return cleanCnpj.Length == 14;
    }
}