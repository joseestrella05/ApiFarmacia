using ApiFarmacia.DAL;
using ApiFarmacia.Dto;
using ApiFarmacia.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ApiFarmacia.Services;

public class UsuarioService : IUsuarioService
{
    private readonly Context _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        Context context,
        IPasswordService passwordService,
        ITokenService tokenService,
        IEmailService emailService,
        IMapper mapper,
        ILogger<UsuarioService> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<(bool Exito, string Mensaje, AuthResponseDto? Data)> RegistrarAsync(RegistroUsuarioDto dto)
    {
        try
        {
            if (!IsValidEmail(dto.Email))
                return (false, "Email inválido", null);

            if (!IsStrongPassword(dto.Password))
                return (false, "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número", null);

            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());

            if (emailExiste)
                return (false, "El email ya está registrado", null);

            // Generar token de confirmación
            var confirmacionToken = GenerarTokenSeguro();

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
                EmailConfirmacionToken = confirmacionToken,
                EmailConfirmacionTokenExpiracion = DateTime.UtcNow.AddHours(24),
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Enviar email de confirmación
            var emailEnviado = await _emailService.EnviarEmailConfirmacionAsync(
                usuario.Email,
                usuario.Nombre,
                confirmacionToken
            );

            if (!emailEnviado)
                _logger.LogWarning("No se pudo enviar email de confirmación a {Email}", usuario.Email);

            // Generar tokens JWT
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

            _logger.LogInformation("Usuario registrado: {Email}", usuario.Email);
            return (true, "Usuario registrado exitosamente. Revisa tu email para confirmar tu cuenta.", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario");
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

            _logger.LogInformation("Login exitoso: {Email}", usuario.Email);
            return (true, "Login exitoso", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión");
            return (false, $"Error al iniciar sesión: {ex.Message}", null);
        }
    }

    public async Task<(bool Exito, string Mensaje)> ConfirmarEmailAsync(string token)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.EmailConfirmacionToken == token);

            if (usuario == null)
                return (false, "Token de confirmación inválido");

            if (usuario.EmailConfirmacionTokenExpiracion < DateTime.UtcNow)
                return (false, "El token de confirmación ha expirado. Solicita uno nuevo.");

            if (usuario.EmailConfirmado)
                return (false, "El email ya ha sido confirmado previamente");

            usuario.EmailConfirmado = true;
            usuario.EmailConfirmacionToken = null;
            usuario.EmailConfirmacionTokenExpiracion = null;
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Enviar email de bienvenida
            await _emailService.EnviarEmailBienvenidaAsync(usuario.Email, usuario.Nombre);

            _logger.LogInformation("Email confirmado: {Email}", usuario.Email);
            return (true, "Email confirmado exitosamente. ¡Bienvenido!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar email");
            return (false, "Error al confirmar email");
        }
    }

    public async Task<(bool Exito, string Mensaje)> ReenviarConfirmacionEmailAsync(string email)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());

            if (usuario == null)
                return (false, "Usuario no encontrado");

            if (usuario.EmailConfirmado)
                return (false, "El email ya está confirmado");

            // Generar nuevo token
            var nuevoToken = GenerarTokenSeguro();
            usuario.EmailConfirmacionToken = nuevoToken;
            usuario.EmailConfirmacionTokenExpiracion = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            // Enviar email
            var emailEnviado = await _emailService.EnviarEmailConfirmacionAsync(
                usuario.Email,
                usuario.Nombre,
                nuevoToken
            );

            if (!emailEnviado)
                return (false, "Error al enviar el email. Intente más tarde.");

            return (true, "Email de confirmación reenviado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reenviar confirmación");
            return (false, "Error al reenviar confirmación");
        }
    }

    public async Task<(bool Exito, string Mensaje)> SolicitarRecuperacionPasswordAsync(string email)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());

            if (usuario == null)
            {
                // Por seguridad, no revelar si el email existe
                return (true, "Si el email existe, recibirás instrucciones para recuperar tu contraseña");
            }

            if (!usuario.Activo)
                return (true, "Si el email existe, recibirás instrucciones para recuperar tu contraseña");

            // Generar token de recuperación
            var resetToken = GenerarTokenSeguro();
            usuario.PasswordResetToken = resetToken;
            usuario.PasswordResetTokenExpiracion = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            // Enviar email
            var emailEnviado = await _emailService.EnviarEmailRecuperacionPasswordAsync(
                usuario.Email,
                usuario.Nombre,
                resetToken
            );

            if (!emailEnviado)
                _logger.LogWarning("No se pudo enviar email de recuperación a {Email}", usuario.Email);

            _logger.LogInformation("Solicitud de recuperación de contraseña: {Email}", usuario.Email);
            return (true, "Si el email existe, recibirás instrucciones para recuperar tu contraseña");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar recuperación de contraseña");
            return (false, "Error al procesar la solicitud");
        }
    }

    public async Task<(bool Exito, string Mensaje)> ResetPasswordAsync(string token, string nuevaPassword)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token);

            if (usuario == null)
                return (false, "Token de recuperación inválido");

            if (usuario.PasswordResetTokenExpiracion < DateTime.UtcNow)
                return (false, "El token de recuperación ha expirado. Solicita uno nuevo.");

            if (!IsStrongPassword(nuevaPassword))
                return (false, "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número");

            usuario.PasswordHash = _passwordService.HashPassword(nuevaPassword);
            usuario.PasswordResetToken = null;
            usuario.PasswordResetTokenExpiracion = null;
            usuario.IntentosAccesoFallidos = 0;
            usuario.BloqueoHasta = null;
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Contraseña restablecida: {Email}", usuario.Email);
            return (true, "Contraseña restablecida exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restablecer contraseña");
            return (false, "Error al restablecer contraseña");
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
            _logger.LogError(ex, "Error al obtener perfil");
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
            _logger.LogError(ex, "Error al actualizar perfil");
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

            if (!IsStrongPassword(dto.PasswordNuevo))
                return (false, "La nueva contraseña no cumple los requisitos de seguridad");

            usuario.PasswordHash = _passwordService.HashPassword(dto.PasswordNuevo);
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (true, "Contraseña actualizada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña");
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
            _logger.LogError(ex, "Error al renovar token");
            return (false, $"Error al renovar token: {ex.Message}", null);
        }
    }

    private static string GenerarTokenSeguro()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) &&
        new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);

    private static bool IsStrongPassword(string password) =>
        password.Length >= 8 &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit);
}