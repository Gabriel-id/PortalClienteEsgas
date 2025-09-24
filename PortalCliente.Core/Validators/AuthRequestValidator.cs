using FluentValidation;
using PortalCliente.Core.Dtos;

namespace PortalCliente.Core.Validators;

public class AuthRequestValidator : AbstractValidator<AuthRequest>
{
    public AuthRequestValidator()
    {
        RuleFor(x => x.ClientNumber)
            .NotEmpty()
            .WithMessage("Número do cliente é obrigatório")
            .Length(1, 20)
            .WithMessage("Número do cliente deve ter entre 1 e 20 caracteres");

        RuleFor(x => x)
            .Must(HaveValidCpfOrCnpj)
            .WithMessage("CPF ou CNPJ deve ser fornecido");

        When(x => !string.IsNullOrEmpty(x.Cpf), () =>
        {
            RuleFor(x => x.Cpf)
                .Must(BeValidCpf)
                .WithMessage("CPF deve ter 11 dígitos");
        });

        When(x => !string.IsNullOrEmpty(x.Cnpj), () =>
        {
            RuleFor(x => x.Cnpj)
                .Must(BeValidCnpj)
                .WithMessage("CNPJ deve ter 14 dígitos");
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