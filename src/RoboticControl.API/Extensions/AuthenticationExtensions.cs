using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RoboticControl.API.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Configure JWT Authentication and Authorization policies
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure JWT Authentication
        // In production, JWT secret MUST be provided via environment variable JWT_SECRET_KEY
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT Secret Key not configured. " +
                "Set JWT_SECRET_KEY environment variable or JwtSettings:SecretKey in appsettings.json");

        // Validate key length for security
        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException(
                $"JWT Secret Key must be at least 32 characters. Current length: {secretKey.Length}");
        }

        var jwtIssuer = configuration["JwtSettings:Issuer"] ?? "RoboticControlAPI";
        var jwtAudience = configuration["JwtSettings:Audience"] ?? "RoboticControlClient";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Configure authorization policies
        services.AddAuthorization(options =>
        {
            // Policy: Both Admin and Operator can operate the robot
            options.AddPolicy("CanOperate", policy =>
                policy.RequireRole("Admin", "Operator"));

            // Policy: Only Admin for critical/dangerous operations
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));
        });

        return services;
    }
}
