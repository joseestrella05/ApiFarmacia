using ApiFarmacia.Dto;

namespace ApiFarmacia.Services;

public interface IUsuarioService
{
    Task<(bool Exito, string Mensaje, AuthResponseDto? Data)> RegistrarAsync(RegistroUsuarioDto dto);
    Task<(bool Exito, string Mensaje, AuthResponseDto? Data)> LoginAsync(LoginDto dto);
    Task<(bool Exito, string Mensaje, UsuarioReadDto? Data)> ObtenerPerfilAsync(int usuarioId);
    Task<(bool Exito, string Mensaje, UsuarioReadDto? Data)> ActualizarPerfilAsync(int usuarioId, ActualizarUsuarioDto dto);
    Task<(bool Exito, string Mensaje)> CambiarPasswordAsync(int usuarioId, CambiarPasswordDto dto);
    Task<(bool Exito, string Mensaje, AuthResponseDto? Data)> RefreshTokenAsync(string refreshToken);
}

