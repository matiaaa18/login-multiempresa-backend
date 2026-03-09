using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(AppDbContext db, ILogger<UsuariosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? estado = "activos", [FromQuery] string? busqueda = null, [FromQuery] string? orden = "nombre")
    {
        var idEmpresa = ObtenerIdEmpresa();
        if (idEmpresa == null)
            return Unauthorized(new ErrorResponse { Mensaje = "Token inválido", Codigo = "INVALID_TOKEN" });

        var query = _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Sexo)
            .Where(u => u.IdEmpresa == idEmpresa.Value);

        if (estado?.ToLower() == "activos")
            query = query.Where(u => u.Vigencia);
        else if (estado?.ToLower() == "inactivos")
            query = query.Where(u => !u.Vigencia);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            query = query.Where(u =>
                u.Nombre.ToLower().Contains(texto) ||
                u.ApellidoPaterno.ToLower().Contains(texto) ||
                (u.ApellidoMaterno != null && u.ApellidoMaterno.ToLower().Contains(texto)) ||
                u.Correo.ToLower().Contains(texto));
        }

        query = orden?.ToLower() switch
        {
            "correo" => query.OrderBy(u => u.Correo),
            "fechaingreso" => query.OrderBy(u => u.FechaIngreso),
            _ => query.OrderBy(u => u.Nombre).ThenBy(u => u.ApellidoPaterno)
        };

        var usuarios = await query
            .Select(u => new UsuarioListItemResponse
            {
                Id = u.Id,
                NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + (u.ApellidoMaterno != null ? " " + u.ApellidoMaterno : ""),
                Correo = u.Correo,
                Sexo = u.Sexo.Nombre,
                FechaIngreso = u.FechaIngreso,
                Vigencia = u.Vigencia
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var idEmpresa = ObtenerIdEmpresa();
        if (idEmpresa == null)
            return Unauthorized(new ErrorResponse { Mensaje = "Token inválido", Codigo = "INVALID_TOKEN" });

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Sexo)
            .FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == idEmpresa.Value);

        if (usuario == null)
            return NotFound(new ErrorResponse { Mensaje = "Usuario no encontrado", Codigo = "USER_NOT_FOUND" });

        return Ok(MapearUsuario(usuario));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request)
    {
        var idEmpresa = ObtenerIdEmpresa();
        if (idEmpresa == null)
            return Unauthorized(new ErrorResponse { Mensaje = "Token inválido", Codigo = "INVALID_TOKEN" });

        var sexoValido = await _db.Sexos.AnyAsync(s => s.Id == request.IdSexo);
        if (!sexoValido)
            return BadRequest(new ErrorResponse { Mensaje = "El sexo seleccionado no es válido", Codigo = "INVALID_SEX" });

        var correoNormalizado = request.Correo.Trim().ToLower();
        var correoExiste = await _db.Usuarios.AnyAsync(u => u.Correo == correoNormalizado);
        if (correoExiste)
            return Conflict(new ErrorResponse { Mensaje = "El correo ya está registrado", Codigo = "EMAIL_ALREADY_EXISTS" });

        var usuario = new Usuario
        {
            IdEmpresa = idEmpresa.Value,
            Nombre = request.Nombre.Trim(),
            ApellidoPaterno = request.ApellidoPaterno.Trim(),
            ApellidoMaterno = string.IsNullOrWhiteSpace(request.ApellidoMaterno) ? null : request.ApellidoMaterno.Trim(),
            FechaNacimiento = NormalizarFecha(request.FechaNacimiento),
            Direccion = string.IsNullOrWhiteSpace(request.Direccion) ? null : request.Direccion.Trim(),
            IdSexo = request.IdSexo,
            Correo = correoNormalizado,
            NombreUsuario = correoNormalizado,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FechaIngreso = DateTime.UtcNow,
            CreadoEn = DateTime.UtcNow,
            Vigencia = true,
            Activo = true,
            UrlImagen = string.IsNullOrWhiteSpace(request.UrlImagen) ? null : request.UrlImagen.Trim(),
            RenovarPwd = false
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var creado = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Sexo)
            .FirstAsync(u => u.Id == usuario.Id);

        _logger.LogInformation("Usuario {UsuarioId} creado en empresa {EmpresaId}", usuario.Id, idEmpresa.Value);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, MapearUsuario(creado));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioRequest request)
    {
        var idEmpresa = ObtenerIdEmpresa();
        if (idEmpresa == null)
            return Unauthorized(new ErrorResponse { Mensaje = "Token inválido", Codigo = "INVALID_TOKEN" });

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == idEmpresa.Value);
        if (usuario == null)
            return NotFound(new ErrorResponse { Mensaje = "Usuario no encontrado", Codigo = "USER_NOT_FOUND" });

        var sexoValido = await _db.Sexos.AnyAsync(s => s.Id == request.IdSexo);
        if (!sexoValido)
            return BadRequest(new ErrorResponse { Mensaje = "El sexo seleccionado no es válido", Codigo = "INVALID_SEX" });

        var correoNormalizado = request.Correo.Trim().ToLower();
        var correoExiste = await _db.Usuarios.AnyAsync(u => u.Correo == correoNormalizado && u.Id != id);
        if (correoExiste)
            return Conflict(new ErrorResponse { Mensaje = "El correo ya pertenece a otro usuario", Codigo = "EMAIL_ALREADY_EXISTS" });

        usuario.Nombre = request.Nombre.Trim();
        usuario.ApellidoPaterno = request.ApellidoPaterno.Trim();
        usuario.ApellidoMaterno = string.IsNullOrWhiteSpace(request.ApellidoMaterno) ? null : request.ApellidoMaterno.Trim();
        usuario.FechaNacimiento = NormalizarFecha(request.FechaNacimiento);
        usuario.Direccion = string.IsNullOrWhiteSpace(request.Direccion) ? null : request.Direccion.Trim();
        usuario.IdSexo = request.IdSexo;
        usuario.Correo = correoNormalizado;
        usuario.NombreUsuario = correoNormalizado;
        usuario.UrlImagen = string.IsNullOrWhiteSpace(request.UrlImagen) ? null : request.UrlImagen.Trim();
        usuario.RenovarPwd = request.RenovarPwd;

        await _db.SaveChangesAsync();

        var actualizado = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Sexo)
            .FirstAsync(u => u.Id == id);

        return Ok(MapearUsuario(actualizado));
    }

    [HttpPatch("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var idEmpresa = ObtenerIdEmpresa();
        if (idEmpresa == null)
            return Unauthorized(new ErrorResponse { Mensaje = "Token inválido", Codigo = "INVALID_TOKEN" });

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == idEmpresa.Value);
        if (usuario == null)
            return NotFound(new ErrorResponse { Mensaje = "Usuario no encontrado", Codigo = "USER_NOT_FOUND" });

        usuario.Vigencia = false;
        usuario.Activo = false;
        await _db.SaveChangesAsync();

        return Ok(new LogoutResponse { Mensaje = "Usuario desactivado" });
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<IActionResult> Reactivar(int id)
    {
        var idEmpresa = ObtenerIdEmpresa();
        if (idEmpresa == null)
            return Unauthorized(new ErrorResponse { Mensaje = "Token inválido", Codigo = "INVALID_TOKEN" });

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == idEmpresa.Value);
        if (usuario == null)
            return NotFound(new ErrorResponse { Mensaje = "Usuario no encontrado", Codigo = "USER_NOT_FOUND" });

        usuario.Vigencia = true;
        usuario.Activo = true;
        await _db.SaveChangesAsync();

        return Ok(new LogoutResponse { Mensaje = "Usuario reactivado" });
    }

    private int? ObtenerIdEmpresa()
    {
        var claim = User.FindFirst("idEmpresa");
        return claim == null ? null : int.Parse(claim.Value);
    }

    private static DateTime? NormalizarFecha(DateTime? fecha)
    {
        if (fecha == null)
            return null;

        return DateTime.SpecifyKind(fecha.Value, DateTimeKind.Utc);
    }

    private static UsuarioResponse MapearUsuario(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno,
            FechaNacimiento = usuario.FechaNacimiento,
            Direccion = usuario.Direccion,
            IdSexo = usuario.IdSexo,
            Sexo = usuario.Sexo.Nombre,
            Correo = usuario.Correo,
            FechaIngreso = usuario.FechaIngreso,
            Vigencia = usuario.Vigencia,
            UrlImagen = usuario.UrlImagen,
            RenovarPwd = usuario.RenovarPwd,
            NombreCompleto = usuario.Nombre + " " + usuario.ApellidoPaterno + (usuario.ApellidoMaterno != null ? " " + usuario.ApellidoMaterno : "")
        };
    }
}