using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class CreateOrderDto
{
    [Required]
    public int UsuarioId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Total { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderProductDto> Productos { get; set; } = new();

    [Required]
    [StringLength(255)]
    public string PaypalOrderId { get; set; } = string.Empty;

    [StringLength(255)]
    public string? PaypalPayerId { get; set; }
}