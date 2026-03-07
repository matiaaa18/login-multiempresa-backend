// DTO: Lo que el backend responde cuando el login es exitoso
namespace Backend.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string NombreEmpresa { get; set; } = "";
    public int IdUsuario { get; set; }
}
