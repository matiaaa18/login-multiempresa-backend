using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Subdominio).HasColumnName("subdominio").IsRequired().HasMaxLength(50);
        builder.Property(e => e.Activa).HasColumnName("activa");
        builder.Property(e => e.CreadaEn).HasColumnName("creada_en").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(e => e.Subdominio).IsUnique();
    }
}
