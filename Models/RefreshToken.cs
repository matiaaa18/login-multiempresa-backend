// Modelo: Espejo de la tabla "refresh_tokens" en PostgreSQL
namespace Backend.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Token { get; set; } = "";
    public DateTime ExpiraEn { get; set; }
    public bool Revocado { get; set; } = false;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // Relación: un refresh token pertenece a un usuario
    public Usuario Usuario { get; set; } = null!;
}
