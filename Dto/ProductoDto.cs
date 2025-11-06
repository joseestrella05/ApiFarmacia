using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class ProductoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(50)]
    public string? Categoria { get; set; }

    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Url(ErrorMessage = "Debe ser una URL válida.")]
    public string? ImagenUrl { get; set; }

}
