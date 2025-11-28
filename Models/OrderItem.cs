using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Models;

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductoId { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int Cantidad { get; set; }

    [Required]
    public decimal Precio { get; set; }

    public virtual Order Order { get; set; } = null!;
}