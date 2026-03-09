namespace Backend.DTOs;

public class UsuarioListItemResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Sexo { get; set; } = "";
    public DateTime FechaIngreso { get; set; }
    public bool Vigencia { get; set; }
}