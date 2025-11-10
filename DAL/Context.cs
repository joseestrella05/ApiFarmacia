using ApiFarmacia.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiFarmacia.DAL;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<Productos> Productos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Productos>(entity =>
        {
            entity.HasKey(e => e.ProductoId);
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId);

            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Apellido)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Telefono)
                .HasMaxLength(20);

            entity.Property(e => e.Rol)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Cliente");

            entity.Property(e => e.Activo)
                .HasDefaultValue(true);

            entity.Property(e => e.EmailConfirmado)
                .HasDefaultValue(false);

            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IntentosAccesoFallidos)
                .HasDefaultValue(0);
        });
    }

}
