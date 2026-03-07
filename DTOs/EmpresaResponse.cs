// DTO: Respuesta del endpoint /empresa/{subdominio}
namespace Backend.DTOs;

public class EmpresaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Subdominio { get; set; } = "";
}
