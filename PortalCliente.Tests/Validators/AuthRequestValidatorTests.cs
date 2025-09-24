using FluentAssertions;
using PortalCliente.Core.Dtos;
using PortalCliente.Core.Validators;

namespace PortalCliente.Tests.Validators;

public class AuthRequestValidatorTests
{
    private readonly AuthRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_ClientNumber_Is_Empty()
    {
        var request = new AuthRequest { ClientNumber = "", Cpf = "12345678901" };
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "ClientNumber");
    }

    [Fact]
    public void Should_Have_Error_When_Both_Cpf_And_Cnpj_Are_Empty()
    {
        var request = new AuthRequest { ClientNumber = "12345", Cpf = "", Cnpj = "" };
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("CPF ou CNPJ deve ser fornecido"));
    }

    [Fact]
    public void Should_Have_Error_When_Cpf_Length_Is_Invalid()
    {
        var request = new AuthRequest { ClientNumber = "12345", Cpf = "123456789", Cnpj = "" };
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Cpf");
    }

    [Fact]
    public void Should_Have_Error_When_Cnpj_Length_Is_Invalid()
    {
        var request = new AuthRequest { ClientNumber = "12345", Cpf = "", Cnpj = "123456789012" };
        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Cnpj");
    }

    [Fact]
    public void Should_Be_Valid_With_Valid_Cpf()
    {
        var request = new AuthRequest { ClientNumber = "12345", Cpf = "12345678901", Cnpj = "" };
        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Be_Valid_With_Valid_Cnpj()
    {
        var request = new AuthRequest { ClientNumber = "12345", Cpf = "", Cnpj = "12345678901234" };
        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}