using Microsoft.AspNetCore.Mvc;
using Moq;
using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Controllers;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_WhenRequestIsNull_ReturnsBadRequest()
    {
        var authServiceMock = new Mock<IAuthService>();
        var controller = new AuthController(authServiceMock.Object);

        var response = await controller.Login(null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ReturnsUnauthorized()
    {
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock
            .Setup(service => service.LoginAsync("test@mail.com", "wrong", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginResponse?)null);

        var controller = new AuthController(authServiceMock.Object);

        var response = await controller.Login(new LoginRequest("test@mail.com", "wrong"), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(response.Result);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsOk()
    {
        var expected = new LoginResponse("token", DateTime.UtcNow.AddMinutes(5));

        var authServiceMock = new Mock<IAuthService>();
        authServiceMock
            .Setup(service => service.LoginAsync("test@mail.com", "Qwer@4321", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new AuthController(authServiceMock.Object);

        var response = await controller.Login(new LoginRequest("test@mail.com", "Qwer@4321"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal(expected, payload);
    }
}
