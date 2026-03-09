namespace Backend.DTOs;

public class UsuarioResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string? ApellidoMaterno { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public int IdSexo { get; set; }
    public string Sexo { get; set; } = "";
    public string Correo { get; set; } = "";
    public DateTime FechaIngreso { get; set; }
    public bool Vigencia { get; set; }
    public string? UrlImagen { get; set; }
    public bool RenovarPwd { get; set; }
    public string NombreCompleto { get; set; } = "";
}