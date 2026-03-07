// ============================================
// TOKEN SERVICE: Genera JWT + Refresh Tokens
// ============================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    // Genera un JWT (access token) con duración de 5 minutos
    public string GenerarAccessToken(int idUsuario, int idEmpresa, string nombreUsuario)
    {
        // FIX: Leer secreto de variable de entorno, con fallback a appsettings
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? _config["Jwt:Secret"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var claims = new[]
        {
            new Claim("idUsuario", idUsuario.ToString()),
            new Claim("idEmpresa", idEmpresa.ToString()),
            new Claim("nombreUsuario", nombreUsuario),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Genera un refresh token (string aleatorio seguro)
    public string GenerarRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
