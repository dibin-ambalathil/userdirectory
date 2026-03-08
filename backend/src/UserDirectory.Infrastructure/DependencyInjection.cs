using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserDirectory.Application.Features.Users.Interfaces;
using UserDirectory.Infrastructure.Persistence;
using UserDirectory.Infrastructure.Repositories;
using UserDirectory.Infrastructure.Seed;

namespace UserDirectory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=/data/app.db";

        services.AddDbContext<UserDirectoryDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDirectoryDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var hostEnvironment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        // Ensure the database directory exists (important for SQLite)
        var dataSource = dbContext.Database.GetDbConnection().DataSource;
        if (!string.IsNullOrWhiteSpace(dataSource))
        {
            var directory = Path.GetDirectoryName(dataSource);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // Run database migrations
        await dbContext.Database.MigrateAsync();

        // Seed initial data (users, roles, etc.)
        await UserSeeder.SeedAsync(dbContext);
        await AuthSeeder.SeedAsync(dbContext, configuration, hostEnvironment);
    }
}
