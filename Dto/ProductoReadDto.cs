namespace ApiFarmacia.Dto;

public class ProductoReadDto
{
    public int ProductoId { get; set; }
    public string? Nombre { get; set; }
    public string? Categoria { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
}
