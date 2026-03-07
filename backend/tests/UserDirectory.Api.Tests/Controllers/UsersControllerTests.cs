using Microsoft.AspNetCore.Mvc;
using Moq;
using UserDirectory.Api.Controllers;
using UserDirectory.Application.Features.Users.DTOs;
using UserDirectory.Application.Features.Users.Interfaces;

namespace UserDirectory.Api.Tests.Controllers;

public sealed class UsersControllerTests
{
    [Fact]
    public async Task GetById_WhenUserMissing_ReturnsNotFound()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithUsers()
    {
        var users = new List<UserDto>
        {
            new(Guid.NewGuid(), "Priya Sharma", 34, "Pune", "Maharashtra", "411001", DateTime.UtcNow)
        };

        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<UserDto>>(okResult.Value);
        Assert.Single(payload);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var createdUser = new UserDto(
            Guid.NewGuid(),
            "Aarav Menon",
            29,
            "Bengaluru",
            "Karnataka",
            "560001",
            DateTime.UtcNow);

        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.CreateAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.Create(new CreateUserRequest
        {
            Name = "Aarav Menon",
            Age = 29,
            City = "Bengaluru",
            State = "Karnataka",
            Pincode = "560001"
        }, CancellationToken.None);

        var result = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(nameof(UsersController.GetById), result.ActionName);
    }
}
