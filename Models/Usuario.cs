// Modelo: Espejo de la tabla "usuarios" en PostgreSQL
namespace Backend.Models;

public class Usuario
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Empresa Empresa { get; set; } = null!;
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
