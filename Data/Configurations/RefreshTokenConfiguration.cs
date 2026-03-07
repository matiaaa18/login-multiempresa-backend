using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.Token).HasColumnName("token").IsRequired();
        builder.Property(e => e.ExpiraEn).HasColumnName("expira_en");
        builder.Property(e => e.Revocado).HasColumnName("revocado");
        builder.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(rt => rt.Usuario)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(rt => rt.IdUsuario)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
