using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UserDirectory.Application.Features.Users.Interfaces;
using UserDirectory.Application.Features.Users.Services;
using UserDirectory.Application.Features.Users.Validators;

namespace UserDirectory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
