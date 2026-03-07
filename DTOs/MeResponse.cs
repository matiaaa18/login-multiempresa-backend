// DTO: Lo que el endpoint /me responde con la info del usuario
namespace Backend.DTOs;

public class MeResponse
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public int IdEmpresa { get; set; }
    public string NombreEmpresa { get; set; } = "";
}
