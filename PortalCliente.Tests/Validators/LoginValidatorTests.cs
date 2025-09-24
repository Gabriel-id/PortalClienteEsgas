using FluentAssertions;
using PortalCliente.Core.Dtos;
using PortalCliente.Core.Validators;

namespace PortalCliente.Tests.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var login = new Login { Username = "", Password = "12345" };
        var result = _validator.Validate(login);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Username");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var login = new Login { Username = "12345678901", Password = "" };
        var result = _validator.Validate(login);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Password");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012345")]
    public void Should_Have_Error_When_Username_Length_Is_Invalid(string username)
    {
        var login = new Login { Username = username, Password = "12345" };
        var result = _validator.Validate(login);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Username");
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("12345678901234")]
    public void Should_Be_Valid_When_Data_Is_Correct(string username)
    {
        var login = new Login { Username = username, Password = "12345" };
        var result = _validator.Validate(login);

        result.IsValid.Should().BeTrue();
    }
}