using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Dto;

public class UpdateOrderStatusDto
{
    [Required]
    [StringLength(50)]
    public string Estado { get; set; } = string.Empty;
}
