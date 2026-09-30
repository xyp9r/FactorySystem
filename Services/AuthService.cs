using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FactorySystem.Data;
using FactorySystem.Endpoints;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;

namespace FactorySystem.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(AppDbContext db, IPasswordHasher hasher, IConfiguration config)
    {
        _config = config;
        _db = db;
        _hasher = hasher;
    }

    public string? Login(LoginDto ldto)
    {
        var user = _db.Users.FirstOrDefault(u => u.Name == ldto.Name);

        if (user == null || !_hasher.VerifyHashedPassword(ldto.Password, user.PasswordHash))
        {
            return null;
        }
        
        var key = _config["JwtSecret"]!;
        var secretBytes = System.Text.Encoding.UTF8.GetBytes(key);
    
        var securityKey = new SymmetricSecurityKey(secretBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.Name, user.Name));
        claims.Add(new Claim(ClaimTypes.Role, user.Role));

        var token = new JwtSecurityToken(issuer: "MyFactory", audience: "MyFactory", claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);
    
        var jwtString = new JwtSecurityTokenHandler().WriteToken(token);
    
        return jwtString;
    }
}