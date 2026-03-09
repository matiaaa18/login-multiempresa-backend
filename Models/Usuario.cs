// Modelo: Espejo de la tabla "usuarios" en PostgreSQL
namespace Backend.Models;

public class Usuario
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public string Nombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string? ApellidoMaterno { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public int IdSexo { get; set; }
    public string Correo { get; set; } = "";
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    public bool Vigencia { get; set; } = true;
    public string? UrlImagen { get; set; }
    public bool RenovarPwd { get; set; } = false;
    public string NombreUsuario { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Empresa Empresa { get; set; } = null!;
    public Sexo Sexo { get; set; } = null!;
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
