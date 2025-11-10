using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class CambiarPasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria")]
    public string? PasswordActual { get; set; } 

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número")]
    public string? PasswordNuevo { get; set; } 
}