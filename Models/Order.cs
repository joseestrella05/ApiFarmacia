using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public decimal Total { get; set; }

    [Required]
    [StringLength(50)]
    public string Estado { get; set; } = "PENDIENTE"; 

    [Required]
    [StringLength(50)]
    public string MetodoPago { get; set; } = "PayPal";

    [StringLength(255)]
    public string? PayPalOrderId { get; set; }

    [StringLength(255)]
    public string? PayPalPayerId { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
