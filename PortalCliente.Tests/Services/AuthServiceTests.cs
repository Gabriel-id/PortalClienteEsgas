using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortalCliente.Core.Dtos;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Core.Services;

namespace PortalCliente.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<ISapService> _sapServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _sapServiceMock = new Mock<ISapService>();
        _authService = new AuthService(_sapServiceMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Success_When_Valid_Credentials()
    {
        var login = new Login { Username = "12345678901", Password = "12345" };
        var expectedResponse = new AuthResponse
        {
            ClientCode = "12345",
            ClientName = "João Silva"
        };

        _sapServiceMock
            .Setup(x => x.Authenticate(It.IsAny<AuthRequest>()))
            .ReturnsAsync(expectedResponse);

        var result = await _authService.Authenticate(login);

        result.Should().NotBeNull();
        result.ClientCode.Should().Be("12345");
        result.ClientName.Should().Be("João Silva");

        _sapServiceMock.Verify(x => x.Authenticate(
            It.Is<AuthRequest>(r => r.ClientNumber == login.Password && r.Cpf == login.Username)), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Call_SAP_With_CNPJ_When_Username_Has_14_Digits()
    {
        var login = new Login { Username = "12345678901234", Password = "12345" };
        var expectedResponse = new AuthResponse
        {
            ClientCode = "12345",
            ClientName = "Empresa LTDA"
        };

        _sapServiceMock
            .Setup(x => x.Authenticate(It.IsAny<AuthRequest>()))
            .ReturnsAsync(expectedResponse);

        await _authService.Authenticate(login);

        _sapServiceMock.Verify(x => x.Authenticate(
            It.Is<AuthRequest>(r => r.ClientNumber == login.Password && r.Cnpj == login.Username)), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Empty_Response_When_SAP_Returns_Empty()
    {
        var login = new Login { Username = "12345678901", Password = "12345" };
        var emptyResponse = new AuthResponse { ClientCode = "", ClientName = "" };

        _sapServiceMock
            .Setup(x => x.Authenticate(It.IsAny<AuthRequest>()))
            .ReturnsAsync(emptyResponse);

        var result = await _authService.Authenticate(login);

        result.Should().NotBeNull();
        result.ClientCode.Should().BeEmpty();
        result.ClientName.Should().BeEmpty();
    }
}