using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class CambiarRolDto
{
    [Required(ErrorMessage = "El nuevo rol es obligatorio")]
    public string NuevoRol { get; set; } = string.Empty;
}

