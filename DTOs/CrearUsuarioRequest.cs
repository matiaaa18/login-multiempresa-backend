using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CrearUsuarioRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El apellido paterno es requerido")]
    [MaxLength(100)]
    public string ApellidoPaterno { get; set; } = "";

    [MaxLength(100)]
    public string? ApellidoMaterno { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    [MaxLength(100)]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "El sexo es requerido")]
    [Range(1, 3, ErrorMessage = "El sexo debe ser 1, 2 o 3")]
    public int IdSexo { get; set; }

    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    [MaxLength(100)]
    public string Correo { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = "";

    [MaxLength(200)]
    public string? UrlImagen { get; set; }
}