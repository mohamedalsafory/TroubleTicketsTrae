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

    public UsersController(IJsonStorageService storage, IConfiguration configuration)
    {
        _storage = storage;
        _configuration = configuration;
        FileName = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");
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
            Email = request.Email,
            Name = request.Name,
            PasswordHash = HashPassword(request.Password),
            Role = "client" // Default role
        };

        users.Add(user);
        await _storage.WriteAsync(FileName, users);

        return Ok(new { token = GenerateJwtToken(user) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var users = await _storage.ReadOrCreateAsync<List<User>>(FileName);
        var user = users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        user.LastLogin = DateTime.UtcNow;
        await _storage.WriteAsync(FileName, users);

        return Ok(new { token = GenerateJwtToken(user) });
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _storage.ReadOrCreateAsync<List<User>>(FileName);
        return Ok(users);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
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