using FluentValidation;
using UserDirectory.Application.Features.Users.DTOs;
using UserDirectory.Application.Features.Users.Interfaces;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Application.Features.Users.Services;

/// <summary>
/// Application service responsible for user CRUD operations.
/// Validates incoming requests and delegates persistence to the repository.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    /// <summary>
    /// Initializes a new instance of <see cref="UserService"/>.
    /// </summary>
    /// <param name="userRepository">Repository for user persistence.</param>
    /// <param name="createValidator">Validator for create requests.</param>
    /// <param name="updateValidator">Validator for update requests.</param>
    public UserService(
        IUserRepository userRepository,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator)
    {
        _userRepository = userRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Retrieves all users from the data store.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of user DTOs.</returns>
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Retrieves a single user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The user DTO if found; otherwise <c>null</c>.</returns>
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapToDto(user);
    }

    /// <summary>
    /// Creates a new user after validating the request.
    /// Throws <see cref="ValidationException"/> if validation fails.
    /// </summary>
    /// <param name="request">The create user request payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created user DTO.</returns>
    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Validate incoming request; throws on failure
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        // Map request to domain entity with a new unique identifier
        var user = new User(
            Guid.NewGuid(),
            request.Name,
            request.Age,
            request.City,
            request.State,
            request.Pincode);

        // Persist entity
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    /// <summary>
    /// Updates an existing user's details after validating the request.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="request">The update user request payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the user was found and updated; otherwise <c>false</c>.</returns>
    public async Task<bool> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Validate incoming request; throws on failure
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        // Fetch existing user; return false if not found
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        // Apply updates via domain method
        user.UpdateDetails(
            request.Name,
            request.Age,
            request.City,
            request.State,
            request.Pincode);

        // Persist changes
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the user was found and deleted; otherwise <c>false</c>.</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Fetch existing user; return false if not found
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        // Remove from data store
        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Maps a domain <see cref="User"/> entity to a <see cref="UserDto"/>.
    /// </summary>
    /// <param name="user">The domain entity.</param>
    /// <returns>The mapped DTO.</returns>
    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Name,
            user.Age,
            user.City,
            user.State,
            user.Pincode,
            user.CreatedAt);
    }
}
