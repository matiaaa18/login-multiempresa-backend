using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class SexoConfiguration : IEntityTypeConfiguration<Sexo>
{
    public void Configure(EntityTypeBuilder<Sexo> builder)
    {
        builder.ToTable("sexos");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(50);

        builder.HasIndex(e => e.Nombre).IsUnique();
    }
}