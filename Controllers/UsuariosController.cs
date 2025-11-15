using ApiFarmacia.Dto;
using ApiFarmacia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ApiFarmacia.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    [HttpPost("registro")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (exito, mensaje, data) = await _usuarioService.RegistrarAsync(dto);

        if (!exito)
            return BadRequest(new { mensaje });

        return CreatedAtAction(nameof(ObtenerPerfil), null, new { mensaje, data });
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (exito, mensaje, data) = await _usuarioService.LoginAsync(dto);

        if (!exito)
            return Unauthorized(new { mensaje });

        return Ok(new { mensaje, data });
    }

    /// <summary>
    /// Confirmar email con token
    /// </summary>
    [HttpGet("confirmar-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmarEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { mensaje = "Token requerido" });

        var (exito, mensaje) = await _usuarioService.ConfirmarEmailAsync(token);

        if (!exito)
            return BadRequest(new { mensaje });

        return Ok(new { mensaje });
    }

    /// <summary>
    /// Reenviar email de confirmación
    /// </summary>
    [HttpPost("reenviar-confirmacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReenviarConfirmacion([FromBody] ReenviarEmailDto dto)
    {
        var (exito, mensaje) = await _usuarioService.ReenviarConfirmacionEmailAsync(dto.Email);

        if (!exito)
            return BadRequest(new { mensaje });

        return Ok(new { mensaje });
    }

    /// <summary>
    /// Solicitar recuperación de contraseña
    /// </summary>
    [HttpPost("solicitar-recuperacion-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SolicitarRecuperacionPassword([FromBody] SolicitarRecuperacionDto dto)
    {
        var (exito, mensaje) = await _usuarioService.SolicitarRecuperacionPasswordAsync(dto.Email);

        // Siempre retornar 200 por seguridad (no revelar si el email existe)
        return Ok(new { mensaje });
    }

    /// <summary>
    /// Restablecer contraseña con token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (exito, mensaje) = await _usuarioService.ResetPasswordAsync(dto.Token, dto.NuevaPassword);

        if (!exito)
            return BadRequest(new { mensaje });

        return Ok(new { mensaje });
    }

    /// <summary>
    /// Obtener perfil del usuario autenticado
    /// </summary>
    [Authorize]
    [HttpGet("perfil")]
    [ProducesResponseType(typeof(UsuarioReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (exito, mensaje, data) = await _usuarioService.ObtenerPerfilAsync(usuarioId);

        if (!exito)
            return NotFound(new { mensaje });

        return Ok(new { mensaje, data });
    }

    /// <summary>
    /// Actualizar perfil del usuario autenticado
    /// </summary>
    [Authorize]
    [HttpPut("perfil")]
    [ProducesResponseType(typeof(UsuarioReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (exito, mensaje, data) = await _usuarioService.ActualizarPerfilAsync(usuarioId, dto);

        if (!exito)
            return NotFound(new { mensaje });

        return Ok(new { mensaje, data });
    }

    /// <summary>
    /// Cambiar contraseña del usuario autenticado
    /// </summary>
    [Authorize]
    [HttpPost("cambiar-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (exito, mensaje) = await _usuarioService.CambiarPasswordAsync(usuarioId, dto);

        if (!exito)
            return BadRequest(new { mensaje });

        return Ok(new { mensaje });
    }

    /// <summary>
    /// Renovar token de acceso usando refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (exito, mensaje, data) = await _usuarioService.RefreshTokenAsync(dto.RefreshToken);

        if (!exito)
            return Unauthorized(new { mensaje });

        return Ok(new { mensaje, data });
    }

    /// <summary>
    /// Obtener información del usuario autenticado
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var nombre = User.FindFirstValue(ClaimTypes.Name);
        var rol = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            usuarioId,
            email,
            nombre,
            rol
        });
    }
}

public class ReenviarEmailDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class SolicitarRecuperacionDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "El token es obligatorio")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número")]
    public string NuevaPassword { get; set; } = string.Empty;
}