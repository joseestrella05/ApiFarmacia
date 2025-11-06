using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Models;

public class Productos
{
    [Key]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "Nombre máximo 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{3,50}$", ErrorMessage = "Solo letras y espacios, mínimo 3 caracteres")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [StringLength(50, ErrorMessage = "Categoría máximo 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{3,30}$", ErrorMessage = "Categoría inválida (solo letras y espacios)")]
    public string? Categoria { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(255, ErrorMessage = "Descripción máximo 255 caracteres")]
    public string? Descripcion { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "Precio fuera de rango")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "La URL de la imagen es obligatoria")]
    [Url(ErrorMessage = "Debe ser una URL válida")]
    [RegularExpression(@"^(http|https):\/\/[\w\-]+(\.[\w\-]+)+[\w\-.,@?^=%&:/~+#]*[\w\-@?^=%&/~+#]$", ErrorMessage = "Debe ser una URL válida")]
    public string? ImagenUrl { get; set; }
}
