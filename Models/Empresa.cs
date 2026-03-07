// Modelo: Espejo de la tabla "empresas" en PostgreSQL
namespace Backend.Models;

public class Empresa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Subdominio { get; set; } = "";
    public bool Activa { get; set; } = true;
    public DateTime CreadaEn { get; set; } = DateTime.UtcNow;

    // Relación: una empresa tiene muchos usuarios
    public List<Usuario> Usuarios { get; set; } = new();
}
