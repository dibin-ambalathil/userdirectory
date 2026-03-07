using Moq;
using UserDirectory.Application.Features.Users.DTOs;
using UserDirectory.Application.Features.Users.Interfaces;
using UserDirectory.Application.Features.Users.Services;
using UserDirectory.Application.Features.Users.Validators;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Application.Tests.Services;

public sealed class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsAndReturnsUser()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserService(
            repositoryMock.Object,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator());

        var result = await service.CreateAsync(new CreateUserRequest
        {
            Name = "Aarav Menon",
            Age = 29,
            City = "Bengaluru",
            State = "Karnataka",
            Pincode = "560001"
        });

        Assert.Equal("Aarav Menon", result.Name);
        Assert.Equal(29, result.Age);

        repositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
        repositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = new UserService(
            repositoryMock.Object,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator());

        var updated = await service.UpdateAsync(Guid.NewGuid(), new UpdateUserRequest
        {
            Name = "Priya Sharma",
            Age = 34,
            City = "Pune",
            State = "Maharashtra",
            Pincode = "411001"
        });

        Assert.False(updated);

        repositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedUsers()
    {
        var users = new List<User>
        {
            new(
                Guid.NewGuid(),
                "Nikhil Das",
                24,
                "Kochi",
                "Kerala",
                "682001",
                DateTime.UtcNow)
        };

        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var service = new UserService(
            repositoryMock.Object,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator());

        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Nikhil Das", result[0].Name);
        Assert.Equal("Kochi", result[0].City);
    }
}
