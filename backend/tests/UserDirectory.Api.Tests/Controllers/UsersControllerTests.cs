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
    public async Task GetById_WhenUserExists_ReturnsOk()
    {
        var user = new UserDto(
            Guid.NewGuid(),
            "Priya Sharma",
            34,
            "Pune",
            "Maharashtra",
            "411001",
            DateTime.UtcNow);

        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(user.Name, payload.Name);
    }

    [Fact]
    public async Task Update_WhenUserExists_ReturnsNoContent()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.Update(Guid.NewGuid(), new UpdateUserRequest
        {
            Name = "Updated Name",
            Age = 35,
            City = "Updated City",
            State = "Updated State",
            Pincode = "123456"
        }, CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task Update_WhenUserMissing_ReturnsNotFound()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.Update(Guid.NewGuid(), new UpdateUserRequest
        {
            Name = "Updated Name",
            Age = 35,
            City = "Updated City",
            State = "Updated State",
            Pincode = "123456"
        }, CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Delete_WhenUserExists_ReturnsNoContent()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task Delete_WhenUserMissing_ReturnsNotFound()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock
            .Setup(service => service.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new UsersController(serviceMock.Object);

        var response = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }
