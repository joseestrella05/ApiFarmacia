using ApiFarmacia.DAL;
using ApiFarmacia.Dto;
using ApiFarmacia.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiFarmacia.Services;

public class UsuarioService : IUsuarioService
{
    private readonly Context _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public UsuarioService(
        Context context,
        IPasswordService passwordService,
        ITokenService tokenService,
        IMapper mapper)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<(bool Exito, string Mensaje, AuthResponseDto? Data)> RegistrarAsync(RegistroUsuarioDto dto)
    {
        try
        {
            
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());

            if (emailExiste)
                return (false, "El email ya está registrado", null);

            
            var usuario = new Usuario
            {
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = _passwordService.HashPassword(dto.Password),
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Telefono = dto.Telefono?.Trim(),
                Rol = "Cliente",
                Activo = true,
                EmailConfirmado = false,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

           
            var token = _tokenService.GenerarToken(usuario);
            var refreshToken = _tokenService.GenerarRefreshToken();

            
            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiracion = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiracion = DateTime.UtcNow.AddHours(1),
                Usuario = _mapper.Map<UsuarioReadDto>(usuario)
            };

            return (true, "Usuario registrado exitosamente", response);
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar usuario: {ex.Message}", null);
        }
    }

    public async Task<(bool Exito, string Mensaje, AuthResponseDto? Data)> LoginAsync(LoginDto dto)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());

            if (usuario == null)
                return (false, "Credenciales inválidas", null);

            
            if (usuario.BloqueoHasta.HasValue && usuario.BloqueoHasta.Value > DateTime.UtcNow)
            {
                var tiempoRestante = (usuario.BloqueoHasta.Value - DateTime.UtcNow).Minutes;
                return (false, $"Cuenta bloqueada. Intente nuevamente en {tiempoRestante} minutos", null);
            }

            
            if (!_passwordService.VerifyPassword(dto.Password, usuario.PasswordHash))
            {
                
                usuario.IntentosAccesoFallidos++;

                if (usuario.IntentosAccesoFallidos >= 5)
                {
                    usuario.BloqueoHasta = DateTime.UtcNow.AddMinutes(15);
                    await _context.SaveChangesAsync();
                    return (false, "Cuenta bloqueada por 15 minutos debido a múltiples intentos fallidos", null);
                }

                await _context.SaveChangesAsync();
                return (false, "Credenciales inválidas", null);
            }

            
            if (!usuario.Activo)
                return (false, "Cuenta desactivada. Contacte al administrador", null);

            
            usuario.IntentosAccesoFallidos = 0;
            usuario.BloqueoHasta = null;
            usuario.UltimoAcceso = DateTime.UtcNow;

            
            var token = _tokenService.GenerarToken(usuario);
            var refreshToken = _tokenService.GenerarRefreshToken();

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiracion = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiracion = DateTime.UtcNow.AddHours(1),
                Usuario = _mapper.Map<UsuarioReadDto>(usuario)
            };

            return (true, "Login exitoso", response);
        }
        catch (Exception ex)
        {
            return (false, $"Error al iniciar sesión: {ex.Message}", null);
        }
    }

    public async Task<(bool Exito, string Mensaje, UsuarioReadDto? Data)> ObtenerPerfilAsync(int usuarioId)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
                return (false, "Usuario no encontrado", null);

            var usuarioDto = _mapper.Map<UsuarioReadDto>(usuario);
            return (true, "Perfil obtenido exitosamente", usuarioDto);
        }
        catch (Exception ex)
        {
            return (false, $"Error al obtener perfil: {ex.Message}", null);
        }
    }

    public async Task<(bool Exito, string Mensaje, UsuarioReadDto? Data)> ActualizarPerfilAsync(
        int usuarioId, ActualizarUsuarioDto dto)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
                return (false, "Usuario no encontrado", null);

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Apellido = dto.Apellido.Trim();
            usuario.Telefono = dto.Telefono?.Trim();
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var usuarioDto = _mapper.Map<UsuarioReadDto>(usuario);
            return (true, "Perfil actualizado exitosamente", usuarioDto);
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar perfil: {ex.Message}", null);
        }
    }

    public async Task<(bool Exito, string Mensaje)> CambiarPasswordAsync(
        int usuarioId, CambiarPasswordDto dto)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
                return (false, "Usuario no encontrado");

            
            if (!_passwordService.VerifyPassword(dto.PasswordActual, usuario.PasswordHash))
                return (false, "La contraseña actual es incorrecta");

            
            usuario.PasswordHash = _passwordService.HashPassword(dto.PasswordNuevo);
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (true, "Contraseña actualizada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al cambiar contraseña: {ex.Message}");
        }
    }

    public async Task<(bool Exito, string Mensaje, AuthResponseDto? Data)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (usuario == null)
                return (false, "Token inválido", null);

            if (usuario.RefreshTokenExpiracion < DateTime.UtcNow)
                return (false, "Token expirado", null);

            if (!usuario.Activo)
                return (false, "Cuenta desactivada", null);

           
            var nuevoToken = _tokenService.GenerarToken(usuario);
            var nuevoRefreshToken = _tokenService.GenerarRefreshToken();

            usuario.RefreshToken = nuevoRefreshToken;
            usuario.RefreshTokenExpiracion = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                Token = nuevoToken,
                RefreshToken = nuevoRefreshToken,
                Expiracion = DateTime.UtcNow.AddHours(1),
                Usuario = _mapper.Map<UsuarioReadDto>(usuario)
            };

            return (true, "Token renovado exitosamente", response);
        }
        catch (Exception ex)
        {
            return (false, $"Error al renovar token: {ex.Message}", null);
        }
    }
}