namespace ApiFarmacia.Dto;

public class UsuarioReadDto
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public bool EmailConfirmado { get; set; }
    public DateTime FechaCreacion { get; set; }
}