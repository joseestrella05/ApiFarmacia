using System.ComponentModel.DataAnnotations;

namespace ApiFarmacia.Models;

public class Usuario
{
    [Key]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

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

    [Required]
    [StringLength(50)]
    public string Rol { get; set; } = "Cliente";

    public bool Activo { get; set; } = true;

    public bool EmailConfirmado { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public int IntentosAccesoFallidos { get; set; } = 0;

    public DateTime? BloqueoHasta { get; set; }

    // Refresh Token
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiracion { get; set; }

    // Token de Confirmación de Email
    public string? EmailConfirmacionToken { get; set; }
    public DateTime? EmailConfirmacionTokenExpiracion { get; set; }

    // Token de Recuperación de Contraseña
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiracion { get; set; }
}