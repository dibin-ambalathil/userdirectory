using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserDirectory.Api.Auth;
using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Auth.Services;
using UserDirectory.Api.Middleware;
using UserDirectory.Application;
using UserDirectory.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCors";

var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

var allowedOrigins = (configuredOrigins is { Length: > 0 }
    ? configuredOrigins
    : new[]
    {
        "http://localhost:5173",
        "https://localhost:5173",
        "http://127.0.0.1:5173",
        "https://127.0.0.1:5173",
        "http://localhost:3000",
        "http://127.0.0.1:3000",
        "https://udtest.netlify.app"
    })
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();
authOptions.Issuer = string.IsNullOrWhiteSpace(authOptions.Issuer) ? "UserDirectory.Api" : authOptions.Issuer.Trim();
authOptions.Audience = string.IsNullOrWhiteSpace(authOptions.Audience) ? "user-directory-api" : authOptions.Audience.Trim();
authOptions.LocalJwtKey = authOptions.LocalJwtKey?.Trim() ?? string.Empty;
authOptions.TokenExpirationMinutes = Math.Max(authOptions.TokenExpirationMinutes, 1);
authOptions.MaxFailedLoginAttempts = Math.Max(authOptions.MaxFailedLoginAttempts, 1);
authOptions.LockoutMinutes = Math.Max(authOptions.LockoutMinutes, 1);
authOptions.LoginRateLimitPerMinute = Math.Max(authOptions.LoginRateLimitPerMinute, 1);

if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(authOptions.LocalJwtKey))
{
    // Keep local development convenient without persisting a reusable key in source files.
    authOptions.LocalJwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

if (string.IsNullOrWhiteSpace(authOptions.LocalJwtKey))
{
    throw new InvalidOperationException("Auth:LocalJwtKey configuration is required.");
}

if (authOptions.LocalJwtKey.Length < 32)
{
    throw new InvalidOperationException("Auth:LocalJwtKey must be at least 32 characters.");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton(authOptions);
builder.Services.AddScoped<IUserCredentialVerifier, DatabaseUserCredentialVerifier>();
builder.Services.AddScoped<IJwtTokenFactory, JwtTokenFactory>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AuthRateLimitPolicies.Login, httpContext =>
    {
        var clientAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"login:{clientAddress}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authOptions.LoginRateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Directory API",
        Version = "v1",
        Description = "API for managing users in the User Directory application."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.LocalJwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize and seed the database on application startup
await app.Services.InitializeDatabaseAsync();

app.Run();

public partial class Program
{
}
