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

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        var user = new User(
            Guid.NewGuid(),
            "Priya Sharma",
            34,
            "Pune",
            "Maharashtra",
            "411001",
            DateTime.UtcNow);

        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = new UserService(
            repositoryMock.Object,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator());

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal("Priya Sharma", result!.Name);
        Assert.Equal("Pune", result.City);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = new UserService(
            repositoryMock.Object,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator());

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesAndReturnsTrue()
    {
        var user = new User(
            Guid.NewGuid(),
            "Old Name",
            25,
            "Old City",
            "Old State",
            "000000",
            DateTime.UtcNow);

        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

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

        Assert.True(updated);
        Assert.Equal("Priya Sharma", user.Name);
        Assert.Equal(34, user.Age);
        Assert.Equal("Pune", user.City);

        repositoryMock.Verify(
            repository => repository.Update(It.IsAny<User>()),
            Times.Once);
        repositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_DeletesAndReturnsTrue()
    {
        var user = new User(
            Guid.NewGuid(),
            "User to Delete",
            30,
            "City",
            "State",
            "123456",
            DateTime.UtcNow);

        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserService(
            repositoryMock.Object,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator());

        var deleted = await service.DeleteAsync(Guid.NewGuid());

        Assert.True(deleted);

        repositoryMock.Verify(
            repository => repository.Remove(It.IsAny<User>()),
            Times.Once);
        repositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = new UserService(
            repositoryMock.Object,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator());

        var deleted = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);

        repositoryMock.Verify(
            repository => repository.Remove(It.IsAny<User>()),
            Times.Never);
        repositoryMock.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
