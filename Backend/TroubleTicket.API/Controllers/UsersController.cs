using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TroubleTicket.Core.Entities;
using TroubleTicket.Core.Interfaces;

namespace TroubleTicket.API.Controllers;

public class UsersController : BaseController
{
    private readonly IJsonStorageService _storage;
    private readonly IConfiguration _configuration;
    private readonly string FileName;
    private bool _usersInitialized = false;

    public UsersController(IJsonStorageService storage, IConfiguration configuration)
    {
        _storage = storage;
        _configuration = configuration;
        FileName = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");
        // Initialize users synchronously
        InitializeUsersAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeUsersAsync()
    {
        if (_usersInitialized) return;
        
        var users = await _storage.ReadOrCreateAsync<List<User>>(FileName);
        if (!users.Any())
        {
            users.Add(new User 
            { 
                Id = Guid.NewGuid().ToString(),
                Name = "Admin",
                Email = "admin@example.com",
                Role = "admin",
                Password = "admin123"
            });
            await _storage.WriteAsync(FileName, users);
        }
        _usersInitialized = true;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var users = await _storage.ReadOrCreateAsync<List<User>>(FileName);
        if (users.Any(u => u.Email == request.Email))
        {
            return BadRequest("Email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            Name = request.Name,
            Password = request.Password,
            Role = "client"
        };

        users.Add(user);
        await _storage.WriteAsync(FileName, users);

        return Ok(new { user.Id, user.Name, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var users = await _storage.ReadOrCreateAsync<List<User>>(FileName);
        var user = users.FirstOrDefault(u => u.Email == request.Email);
        
        if (user == null || user.Password != request.Password)
        {
            return Unauthorized("Invalid email or password");
        }
        
        user.LastLogin = DateTime.UtcNow;
        await _storage.WriteAsync(FileName, users);
        
        return Ok(new { user.Id, user.Name, user.Email, user.Role });
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _storage.ReadOrCreateAsync<List<User>>(FileName);
        return Ok(users);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}