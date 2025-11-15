using ApiFarmacia.DAL;
using ApiFarmacia.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiFarmacia.Controllers;

[Authorize(Roles = "Administrador")]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly Context _context;
    private readonly IMapper _mapper;

    public AdminController(Context context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtener todos los usuarios (solo administradores)
    /// </summary>
    [HttpGet("usuarios")]
    [ProducesResponseType(typeof(IEnumerable<UsuarioReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UsuarioReadDto>>> GetUsuarios()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        var usuariosDto = _mapper.Map<IEnumerable<UsuarioReadDto>>(usuarios);

        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        Console.WriteLine($"[AUDIT] Admin {adminEmail} consultó lista de usuarios");

        return Ok(new
        {
            mensaje = "Lista de usuarios obtenida",
            total = usuarios.Count,
            data = usuariosDto
        });
    }

    /// <summary>
    /// Obtener un usuario específico por ID
    /// </summary>
    [HttpGet("usuarios/{id}")]
    [ProducesResponseType(typeof(UsuarioReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioReadDto>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado" });

        var usuarioDto = _mapper.Map<UsuarioReadDto>(usuario);
        return Ok(new { mensaje = "Usuario encontrado", data = usuarioDto });
    }

    /// <summary>
    /// Cambiar el rol de un usuario
    /// </summary>
    [HttpPut("usuarios/{id}/rol")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado" });

        // Validar roles válidos
        var rolesValidos = new[] { "Cliente", "Empleado", "Administrador" };
        if (!rolesValidos.Contains(dto.NuevoRol))
            return BadRequest(new
            {
                mensaje = "Rol inválido",
                rolesValidos = rolesValidos
            });

        var rolAnterior = usuario.Rol;
        usuario.Rol = dto.NuevoRol;
        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log de auditoría
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        var adminNombre = User.FindFirstValue(ClaimTypes.Name);
        Console.WriteLine($"[AUDIT] Admin {adminNombre} ({adminEmail}) cambió rol de {usuario.Email}: {rolAnterior} → {dto.NuevoRol}");

        return Ok(new
        {
            mensaje = $"Rol actualizado de '{rolAnterior}' a '{dto.NuevoRol}'",
            data = _mapper.Map<UsuarioReadDto>(usuario)
        });
    }

    /// <summary>
    /// Activar o desactivar un usuario
    /// </summary>
    [HttpPut("usuarios/{id}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado" });

        usuario.Activo = dto.Activo;
        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log de auditoría
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        var estado = dto.Activo ? "activó" : "desactivó";
        Console.WriteLine($"[AUDIT] Admin {adminEmail} {estado} usuario: {usuario.Email}");

        return Ok(new
        {
            mensaje = $"Usuario {(dto.Activo ? "activado" : "desactivado")} exitosamente",
            data = _mapper.Map<UsuarioReadDto>(usuario)
        });
    }

    /// <summary>
    /// Eliminar un usuario (soft delete recomendado, aquí es permanente)
    /// </summary>
    [HttpDelete("usuarios/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado" });

        // No permitir que el admin se elimine a sí mismo
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == adminId)
            return BadRequest(new { mensaje = "No puedes eliminar tu propia cuenta" });

        var emailEliminado = usuario.Email;
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        // Log de auditoría
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        Console.WriteLine($"[AUDIT] Admin {adminEmail} eliminó usuario: {emailEliminado}");

        return NoContent();
    }

    /// <summary>
    /// Obtener estadísticas del sistema
    /// </summary>
    [HttpGet("estadisticas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadisticas()
    {
        var totalUsuarios = await _context.Usuarios.CountAsync();
        var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo);
        var usuariosPorRol = await _context.Usuarios
            .GroupBy(u => u.Rol)
            .Select(g => new { rol = g.Key, cantidad = g.Count() })
            .ToListAsync();

        var totalProductos = await _context.Productos.CountAsync();
        var productosPorCategoria = await _context.Productos
            .GroupBy(p => p.Categoria)
            .Select(g => new { categoria = g.Key, cantidad = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            mensaje = "Estadísticas del sistema",
            data = new
            {
                usuarios = new
                {
                    total = totalUsuarios,
                    activos = usuariosActivos,
                    inactivos = totalUsuarios - usuariosActivos,
                    porRol = usuariosPorRol
                },
                productos = new
                {
                    total = totalProductos,
                    porCategoria = productosPorCategoria
                }
            }
        });
    }

    /// <summary>
    /// Resetear intentos de acceso fallidos de un usuario
    /// </summary>
    [HttpPost("usuarios/{id}/desbloquear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesbloquearUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado" });

        usuario.IntentosAccesoFallidos = 0;
        usuario.BloqueoHasta = null;
        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        Console.WriteLine($"[AUDIT] Admin {adminEmail} desbloqueó usuario: {usuario.Email}");

        return Ok(new
        {
            mensaje = "Usuario desbloqueado exitosamente",
            data = _mapper.Map<UsuarioReadDto>(usuario)
        });
    }
}

