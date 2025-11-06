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

    // GET: api/Productos
    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<ProductoReadDto>>> GetProductos()
    {
        var productos = await _context.Productos.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<ProductoReadDto>>(productos));
    }

    // GET: api/Productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoReadDto>> GetProductos(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto == null)
            return NotFound();

        return Ok(_mapper.Map<ProductoReadDto>(producto));
    }


    // PUT: api/Productos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProductos(int id, Productos productos)
    {
        if (id != productos.ProductoId)
        {
            return BadRequest();
        }

        _context.Entry(productos).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductosExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Productos
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult<ProductoReadDto>> PostProductos([FromBody] ProductoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existe = await _context.Productos
            .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());

        if (existe)
            return Conflict("Ya existe un producto con ese nombre.");

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

        var productoReadDto = new ProductoReadDto
        {
            ProductoId = producto.ProductoId,
            Nombre = producto.Nombre,
            Categoria = producto.Categoria,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            ImagenUrl = producto.ImagenUrl
        };

        return CreatedAtAction(nameof(GetProductos), new { id = producto.ProductoId }, productoReadDto);
    }

    // DELETE: api/Productos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductos(int id)
    {
        var productos = await _context.Productos.FindAsync(id);
        if (productos == null)
        {
            return NotFound();
        }

        _context.Productos.Remove(productos);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductosExists(int id)
    {
        return _context.Productos.Any(e => e.ProductoId == id);
    }
}
