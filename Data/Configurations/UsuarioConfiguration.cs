using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
        builder.Property(e => e.NombreUsuario).HasColumnName("nombre_usuario").IsRequired().HasMaxLength(100);
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(e => e.Activo).HasColumnName("activo");
        builder.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(u => u.Empresa)
               .WithMany(e => e.Usuarios)
               .HasForeignKey(u => u.IdEmpresa)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.IdEmpresa, u.NombreUsuario }).IsUnique();
    }
}
