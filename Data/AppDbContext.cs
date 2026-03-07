// ============================================
// DbContext: La CONEXIÓN entre C# y PostgreSQL
// ============================================
// Usa Entity Configurations (IEntityTypeConfiguration<T>) para
// separar la configuración de cada entidad en su propio archivo.

using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Cada DbSet es una "puerta" a una tabla
    public DbSet<Models.Empresa> Empresas { get; set; }
    public DbSet<Models.Usuario> Usuarios { get; set; }
    public DbSet<Models.RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica TODAS las configuraciones de la carpeta Configurations/
        // automáticamente (EmpresaConfiguration, UsuarioConfiguration, etc.)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
