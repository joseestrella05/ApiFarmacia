using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class ActualizarUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,100}$", ErrorMessage = "Solo letras y espacios")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,100}$", ErrorMessage = "Solo letras y espacios")]
    public string Apellido { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Teléfono inválido")]
    [StringLength(20)]
    public string? Telefono { get; set; }
}