using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class RefreshTokenDto
{
    [Required]
    public string? RefreshToken { get; set; } 
}
