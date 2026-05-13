using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class JwtProvider : IJwtProvider
{
    private readonly string _secret;
    private readonly int _accessTokenLifetimeMinutes = 30;
    private readonly int _refreshTokenLifetimeDays = 7;

    public JwtProvider(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret not found");
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "User")
        };

        // Временная проверка админа
        if (user.Email?.ToLower() == "admin@skillswap.ru" ||
            user.Name?.ToLower().Contains("admin") == true)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var plainRefreshToken = Convert.ToBase64String(randomBytes);

        return new RefreshToken
        {
            UserId = userId,
            Token = BCrypt.Net.BCrypt.HashPassword(plainRefreshToken), // храним хэш
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenLifetimeDays)
        };
    }

    // === Новый метод ===
    public Guid? ValidateAccessToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : null;
        }
        catch
        {
            return null; // Токен недействителен или истёк
        }
    }
}












//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;
//using AuthService.Application.Interfaces;
//using AuthService.Domain.Entities;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;

//namespace AuthService.Infrastructure.Services;

//public class JwtProvider : IJwtProvider
//{
//    private readonly string _secret;
//    private readonly int _accessTokenLifetimeMinutes = 30;   // 30 минут
//    private readonly int _refreshTokenLifetimeDays = 7;      // 7 дней

//    public JwtProvider(IConfiguration configuration)
//    {
//        _secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not found");
//    }

//    public string GenerateAccessToken(User user)
//    {
//        var claims = new List<Claim>
//        {
//            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//            new Claim(ClaimTypes.Name, user.Name),
//            new Claim(ClaimTypes.Email, user.Email),
//            new Claim(ClaimTypes.Role, "User")
//        };

//        // Админ (временно)
//        if (user.Email == "admin@skillswap.ru" || user.Name.ToLower().Contains("admin"))
//            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

//        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
//        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//        var token = new JwtSecurityToken(
//            claims: claims,
//            expires: DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes),
//            signingCredentials: creds);

//        return new JwtSecurityTokenHandler().WriteToken(token);
//    }

//    public RefreshToken GenerateRefreshToken(Guid userId)
//    {
//        var randomBytes = RandomNumberGenerator.GetBytes(64);
//        var refreshToken = Convert.ToBase64String(randomBytes);

//        return new RefreshToken
//        {
//            UserId = userId,
//            Token = BCrypt.Net.BCrypt.HashPassword(refreshToken), // храним хэш!
//            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenLifetimeDays)
//        };
//    }
//}












////using System.IdentityModel.Tokens.Jwt;
////using System.Security.Claims;
////using System.Text;
////using AuthService.Application.Interfaces;
////using AuthService.Domain.Entities;
////using Microsoft.Extensions.Configuration;
////using Microsoft.IdentityModel.Tokens;

////namespace AuthService.Infrastructure.Services;

////public class JwtProvider : IJwtProvider
////{
////    private readonly IConfiguration _configuration;

////    public JwtProvider(IConfiguration configuration)
////    {
////        _configuration = configuration;
////    }

////    public string Generate(User user)
////    {
////        var secret = _configuration["Jwt:Secret"]
////                     ?? "super-secret-key-minimum-32-chars!";
////        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

////        var claims = new[]
////        {
////            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
////            new Claim(ClaimTypes.Email, user.Email),
////            new Claim(ClaimTypes.Name, user.Name)
////        };

////        var token = new JwtSecurityToken(
////            claims: claims,
////            expires: DateTime.UtcNow.AddDays(30),
////            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
////        );

////        return new JwtSecurityTokenHandler().WriteToken(token);
////    }
////}