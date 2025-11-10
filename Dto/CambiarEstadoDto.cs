using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class CambiarEstadoDto
{
    [Required]
    public bool Activo { get; set; }
}
