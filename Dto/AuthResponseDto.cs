namespace ApiFarmacia.Dto;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public UsuarioReadDto Usuario { get; set; } = null!;
}