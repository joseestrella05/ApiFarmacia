using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiFarmacia.DAL;
using ApiFarmacia.Models;
using ApiFarmacia.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiFarmacia.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductosController : ControllerBase
{
    private readonly Context _context;
    private readonly IMapper _mapper;

    public ProductosController(Context context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtener todos los productos
    /// </summary>
    /// <remarks>
    /// Público - No requiere autenticación
    /// </remarks>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<ProductoReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductoReadDto>>> GetProductos()
    {
        var productos = await _context.Productos.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<ProductoReadDto>>(productos));
    }

    /// <summary>
    /// Obtener un producto por ID
    /// </summary>
    /// <remarks>
    /// Público - No requiere autenticación
    /// </remarks>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductoReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoReadDto>> GetProductos(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado" });

        return Ok(_mapper.Map<ProductoReadDto>(producto));
    }

    /// <summary>
    /// Actualizar un producto
    /// </summary>
    /// <remarks>
    /// Requiere autenticación - Solo Empleados y Administradores
    /// </remarks>
    [Authorize(Roles = "Empleado,Administrador")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutProductos(int id, Productos productos)
    {
        if (id != productos.ProductoId)
            return BadRequest(new { mensaje = "El ID no coincide" });

        _context.Entry(productos).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();

            var usuarioEmail = User.FindFirstValue(ClaimTypes.Email);
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"[AUDIT] Usuario {usuarioEmail} (ID: {usuarioId}) actualizó producto ID {id}");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductosExists(id))
                return NotFound(new { mensaje = "Producto no encontrado" });
            else
                throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Crear un nuevo producto
    /// </summary>
    /// <remarks>
    /// Requiere autenticación - Solo Empleados y Administradores
    /// </remarks>
    [Authorize(Roles = "Empleado,Administrador")]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductoReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoReadDto>> PostProductos([FromBody] ProductoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existe = await _context.Productos
            .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());

        if (existe)
            return Conflict(new { mensaje = "Ya existe un producto con ese nombre" });

        var producto = new Productos
        {
            Nombre = dto.Nombre,
            Categoria = dto.Categoria,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            ImagenUrl = dto.ImagenUrl
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        // Log de auditoría
        var usuarioEmail = User.FindFirstValue(ClaimTypes.Email);
        var usuarioNombre = User.FindFirstValue(ClaimTypes.Name);
        var rol = User.FindFirstValue(ClaimTypes.Role);
        Console.WriteLine($"[AUDIT] {rol} {usuarioNombre} ({usuarioEmail}) creó producto: {producto.Nombre}");

        var productoReadDto = _mapper.Map<ProductoReadDto>(producto);

        return CreatedAtAction(nameof(GetProductos), new { id = producto.ProductoId }, new
        {
            mensaje = "Producto creado exitosamente",
            data = productoReadDto
        });
    }

    /// <summary>
    /// Eliminar un producto
    /// </summary>
    /// <remarks>
    /// Requiere autenticación - Solo Administradores
    /// </remarks>
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductos(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado" });

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        // Log de auditoría
        var usuarioEmail = User.FindFirstValue(ClaimTypes.Email);
        var usuarioNombre = User.FindFirstValue(ClaimTypes.Name);
        Console.WriteLine($"[AUDIT] Administrador {usuarioNombre} ({usuarioEmail}) eliminó producto: {producto.Nombre}");

        return NoContent();
    }

    /// <summary>
    /// Obtener productos por categoría (ejemplo de endpoint público con filtro)
    /// </summary>
    [HttpGet("categoria/{categoria}")]
    [ProducesResponseType(typeof(IEnumerable<ProductoReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductoReadDto>>> GetProductosPorCategoria(string categoria)
    {
        var productos = await _context.Productos
            .Where(p => p.Categoria.ToLower() == categoria.ToLower())
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ProductoReadDto>>(productos));
    }

    /// <summary>
    /// Obtener información del usuario actual (ejemplo de uso de Claims)
    /// </summary>
    [Authorize]
    [HttpGet("mi-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMiInfo()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var nombre = User.FindFirstValue(ClaimTypes.Name);
        var rol = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            mensaje = "Información del usuario autenticado",
            data = new
            {
                usuarioId,
                email,
                nombre,
                rol,
                estaAutenticado = User.Identity?.IsAuthenticated ?? false,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            }
        });
    }

    private bool ProductosExists(int id)
    {
        return _context.Productos.Any(e => e.ProductoId == id);
    }
}