using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace RoboticControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    // In-memory user store with BCrypt hashed passwords
    private static readonly Dictionary<string, User> _users = new()
    {
        ["admin"] = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin"
        },
        ["operator"] = new User
        {
            Username = "operator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("operator123"),
            Role = "Operator"
        }
    };

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        // Check if user exists
        if (!_users.TryGetValue(request.Username, out var user))
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Verify password using BCrypt
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password - {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Generate JWT token
        var token = GenerateToken(user.Username, user.Role);

        _logger.LogInformation("Login successful: {Username}", request.Username);

        return Ok(new
        {
            token,
            username = user.Username,
            role = user.Role,
            expiresAt = DateTime.UtcNow.AddHours(8)
        });
    }

    private string GenerateToken(string username, string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]
                ?? "YourSuperSecretKeyThatShouldBe32CharsOrMore!"));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "RoboticControlAPI",
            audience: "RoboticControlClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Username, string Password);

public class User
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
