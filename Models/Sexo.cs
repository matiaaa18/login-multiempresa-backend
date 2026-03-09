namespace Backend.Models;

public class Sexo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";

    public List<Usuario> Usuarios { get; set; } = new();
}