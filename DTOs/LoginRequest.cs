// DTO: Lo que el frontend envía para hacer login
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [MinLength(3, ErrorMessage = "El usuario debe tener al menos 3 caracteres")]
    public string NombreUsuario { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(1, ErrorMessage = "La contraseña no puede estar vacía")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "El subdominio es requerido")]
    public string Subdominio { get; set; } = "";
}
