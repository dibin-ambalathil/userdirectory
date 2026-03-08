using Moq;
using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Auth.Models;
using UserDirectory.Api.Auth.Services;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Tests.Auth;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WhenCredentialsInvalid_ReturnsNull()
    {
        var verifierMock = new Mock<IUserCredentialVerifier>();
        verifierMock
            .Setup(verifier => verifier.VerifyAsync("test@mail.com", "wrong", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticatedUser?)null);

        var tokenFactoryMock = new Mock<IJwtTokenFactory>();

        var service = new AuthService(verifierMock.Object, tokenFactoryMock.Object);

        var result = await service.LoginAsync("test@mail.com", "wrong", CancellationToken.None);

        Assert.Null(result);
        tokenFactoryMock.Verify(factory => factory.CreateToken(It.IsAny<AuthenticatedUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsValid_ReturnsTokenFromFactory()
    {
        var user = new AuthenticatedUser(Guid.NewGuid(), "test@mail.com", new[] { "User" });
        var expected = new LoginResponse("token", DateTime.UtcNow.AddMinutes(5));

        var verifierMock = new Mock<IUserCredentialVerifier>();
        verifierMock
            .Setup(verifier => verifier.VerifyAsync("test@mail.com", "Qwer@4321", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var tokenFactoryMock = new Mock<IJwtTokenFactory>();
        tokenFactoryMock
            .Setup(factory => factory.CreateToken(user))
            .Returns(expected);

        var service = new AuthService(verifierMock.Object, tokenFactoryMock.Object);

        var result = await service.LoginAsync("test@mail.com", "Qwer@4321", CancellationToken.None);

        Assert.Equal(expected, result);
        tokenFactoryMock.Verify(factory => factory.CreateToken(user), Times.Once);
    }
}
