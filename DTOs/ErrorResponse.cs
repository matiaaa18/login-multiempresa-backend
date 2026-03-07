// DTO: Respuesta de error estandarizada (en vez de objetos anónimos)
namespace Backend.DTOs;

public class ErrorResponse
{
    public string Mensaje { get; set; } = "";
    public string? Codigo { get; set; }
}
