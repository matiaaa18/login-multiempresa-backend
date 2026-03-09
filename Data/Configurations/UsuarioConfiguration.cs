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
        builder.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100);
        builder.Property(e => e.ApellidoPaterno).HasColumnName("apellido_paterno").HasMaxLength(100);
        builder.Property(e => e.ApellidoMaterno).HasColumnName("apellido_materno").HasMaxLength(100);
        builder.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
        builder.Property(e => e.Direccion).HasColumnName("direccion").HasMaxLength(100);
        builder.Property(e => e.IdSexo).HasColumnName("id_sexo");
        builder.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(100);
        builder.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Vigencia).HasColumnName("vigencia");
        builder.Property(e => e.UrlImagen).HasColumnName("url_imagen").HasMaxLength(200);
        builder.Property(e => e.RenovarPwd).HasColumnName("renovar_pwd");
        builder.Property(e => e.NombreUsuario).HasColumnName("nombre_usuario").IsRequired().HasMaxLength(100);
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(e => e.Activo).HasColumnName("activo");
        builder.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(u => u.Empresa)
               .WithMany(e => e.Usuarios)
               .HasForeignKey(u => u.IdEmpresa)
               .OnDelete(DeleteBehavior.Restrict);

         builder.HasOne(u => u.Sexo)
             .WithMany(s => s.Usuarios)
             .HasForeignKey(u => u.IdSexo)
             .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.IdEmpresa, u.NombreUsuario }).IsUnique();
         builder.HasIndex(u => u.Correo).IsUnique();
    }
}
