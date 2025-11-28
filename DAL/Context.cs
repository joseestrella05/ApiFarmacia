using ApiFarmacia.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiFarmacia.DAL;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<Productos> Productos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

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
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Rol).IsRequired().HasMaxLength(50).HasDefaultValue("Cliente");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.EmailConfirmado).HasDefaultValue(false);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IntentosAccesoFallidos).HasDefaultValue(0);
            entity.HasIndex(e => e.EmailConfirmacionToken);
            entity.HasIndex(e => e.PasswordResetToken);
            entity.HasIndex(e => e.RefreshToken);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MetodoPago).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId);
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
