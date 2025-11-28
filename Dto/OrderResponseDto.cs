namespace ApiFarmacia.Dto;

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<OrderProductDto> Productos { get; set; } = new();
    public string? PaypalOrderId { get; set; }
    public string? PaypalPayerId { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
    public string FechaActualizacion { get; set; } = string.Empty;
}