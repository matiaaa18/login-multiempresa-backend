// DTO: Lo que el frontend envía para renovar el token
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class RefreshRequest
{
    [Required(ErrorMessage = "El refresh token es requerido")]
    public string RefreshToken { get; set; } = "";
}
