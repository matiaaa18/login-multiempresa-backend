// ============================================
// AUTH CONTROLLER: Las "puertas" del backend
// ============================================
// Endpoints:
//   POST /api/auth/login   → iniciar sesión
//   POST /api/auth/refresh → renovar token
//   GET  /api/auth/me      → ver info del usuario logueado
//   POST /api/auth/logout  → cerrar sesión
//   GET  /api/auth/empresa/{subdominio} → detectar empresa

using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext db, TokenService tokenService, ILogger<AuthController> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    // ============================================
    // POST /api/auth/login
    // ============================================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var identificador = string.IsNullOrWhiteSpace(request.Correo)
                ? request.NombreUsuario?.Trim().ToLower()
                : request.Correo.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(identificador))
            {
                return BadRequest(new ErrorResponse
                {
                    Mensaje = "Debes ingresar correo o nombre de usuario",
                    Codigo = "MISSING_LOGIN_IDENTIFIER"
                });
            }

            // PASO 1: Buscar empresa por subdominio
            var empresa = await _db.Empresas
                .FirstOrDefaultAsync(e => e.Subdominio == request.Subdominio.ToLower().Trim());

            // FIX: Validar que la empresa exista Y esté activa
            if (empresa == null || !empresa.Activa)
                return NotFound(new ErrorResponse
                {
                    Mensaje = "Empresa no encontrada",
                    Codigo = "EMPRESA_NOT_FOUND"
                });

            // PASO 2: Buscar usuario en esa empresa
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdEmpresa == empresa.Id &&
                    (u.Correo.ToLower() == identificador || u.NombreUsuario.ToLower() == identificador));

            // FIX: Validar que el usuario exista Y esté activo
            if (usuario == null || !usuario.Activo || !usuario.Vigencia)
                return Unauthorized(new ErrorResponse
                {
                    Mensaje = "Usuario o contraseña inválidos",
                    Codigo = "INVALID_CREDENTIALS"
                });

            // PASO 3: Verificar contraseña con BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
                return Unauthorized(new ErrorResponse
                {
                    Mensaje = "Usuario o contraseña inválidos",
                    Codigo = "INVALID_CREDENTIALS"
                });

            // PASO 4: Generar tokens
            var accessToken = _tokenService.GenerarAccessToken(
                usuario.Id, empresa.Id, usuario.NombreUsuario);
            var refreshTokenStr = _tokenService.GenerarRefreshToken();

            // Guardar refresh token en la BD
            var refreshToken = new RefreshToken
            {
                IdUsuario = usuario.Id,
                Token = refreshTokenStr,
                ExpiraEn = DateTime.UtcNow.AddDays(7),
                Revocado = false
            };
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Login exitoso: usuario {UsuarioId} de empresa {EmpresaId}",
                usuario.Id, empresa.Id);

            // PASO 5: Responder con DTOs tipados (no objetos anónimos)
            return Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenStr,
                NombreEmpresa = empresa.Nombre,
                IdUsuario = usuario.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login para subdominio {Subdominio}", request.Subdominio);
            return StatusCode(500, new ErrorResponse
            {
                Mensaje = "Error interno del servidor",
                Codigo = "INTERNAL_ERROR"
            });
        }
    }

    // ============================================
    // POST /api/auth/refresh
    // ============================================
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var storedToken = await _db.RefreshTokens
                .Include(rt => rt.Usuario)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null || storedToken.Revocado || storedToken.ExpiraEn < DateTime.UtcNow)
                return Unauthorized(new ErrorResponse
                {
                    Mensaje = "Refresh token inválido o expirado",
                    Codigo = "INVALID_REFRESH_TOKEN"
                });

            var usuario = storedToken.Usuario;
            var empresa = await _db.Empresas.FindAsync(usuario.IdEmpresa);

            if (empresa == null)
                return Unauthorized(new ErrorResponse
                {
                    Mensaje = "Empresa no encontrada",
                    Codigo = "EMPRESA_NOT_FOUND"
                });

            var newAccessToken = _tokenService.GenerarAccessToken(
                usuario.Id, empresa.Id, usuario.NombreUsuario);

            // FIX: Usar DTO tipado en vez de objeto anónimo
            return Ok(new RefreshResponse { AccessToken = newAccessToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en refresh token");
            return StatusCode(500, new ErrorResponse
            {
                Mensaje = "Error interno del servidor",
                Codigo = "INTERNAL_ERROR"
            });
        }
    }

    // ============================================
    // GET /api/auth/me
    // ============================================
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            // FIX: Validar claims en vez de usar null-forgiving operator (!)
            var idUsuarioClaim = User.FindFirst("idUsuario");
            var idEmpresaClaim = User.FindFirst("idEmpresa");

            if (idUsuarioClaim == null || idEmpresaClaim == null)
                return Unauthorized(new ErrorResponse
                {
                    Mensaje = "Token inválido: claims no encontrados",
                    Codigo = "INVALID_TOKEN_CLAIMS"
                });

            var idUsuario = int.Parse(idUsuarioClaim.Value);
            var idEmpresa = int.Parse(idEmpresaClaim.Value);

            var empresa = await _db.Empresas.FindAsync(idEmpresa);
            var usuario = await _db.Usuarios.FindAsync(idUsuario);

            if (empresa == null || usuario == null)
                return NotFound(new ErrorResponse
                {
                    Mensaje = "Usuario o empresa no encontrados",
                    Codigo = "NOT_FOUND"
                });

            return Ok(new MeResponse
            {
                IdUsuario = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.Nombre + " " + usuario.ApellidoPaterno + (usuario.ApellidoMaterno != null ? " " + usuario.ApellidoMaterno : ""),
                Correo = usuario.Correo,
                IdEmpresa = empresa.Id,
                NombreEmpresa = empresa.Nombre
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint /me");
            return StatusCode(500, new ErrorResponse
            {
                Mensaje = "Error interno del servidor",
                Codigo = "INTERNAL_ERROR"
            });
        }
    }

    // ============================================
    // POST /api/auth/logout
    // ============================================
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        try
        {
            var storedToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken != null)
            {
                storedToken.Revocado = true;
                await _db.SaveChangesAsync();
                _logger.LogInformation("Refresh token revocado para usuario {UsuarioId}", storedToken.IdUsuario);
            }

            // FIX: Limpiar tokens expirados de paso (mantenimiento)
            var tokensExpirados = await _db.RefreshTokens
                .Where(rt => rt.ExpiraEn < DateTime.UtcNow)
                .CountAsync();

            if (tokensExpirados > 0)
            {
                await _db.RefreshTokens
                    .Where(rt => rt.ExpiraEn < DateTime.UtcNow)
                    .ExecuteDeleteAsync();
                _logger.LogInformation("Limpiados {Count} refresh tokens expirados", tokensExpirados);
            }

            // FIX: Usar DTO tipado en vez de objeto anónimo
            return Ok(new LogoutResponse { Mensaje = "Sesión cerrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en logout");
            return StatusCode(500, new ErrorResponse
            {
                Mensaje = "Error interno del servidor",
                Codigo = "INTERNAL_ERROR"
            });
        }
    }

    // ============================================
    // GET /api/auth/empresa/{subdominio}
    // ============================================
    [HttpGet("empresa/{subdominio}")]
    public async Task<IActionResult> GetEmpresa(string subdominio)
    {
        var empresa = await _db.Empresas
            .FirstOrDefaultAsync(e => e.Subdominio == subdominio.ToLower().Trim());

        if (empresa == null || !empresa.Activa)
            return NotFound(new ErrorResponse
            {
                Mensaje = "Empresa no encontrada",
                Codigo = "EMPRESA_NOT_FOUND"
            });

        // FIX: Usar DTO tipado en vez de objeto anónimo
        return Ok(new EmpresaResponse
        {
            Id = empresa.Id,
            Nombre = empresa.Nombre,
            Subdominio = empresa.Subdominio
        });
    }
}
