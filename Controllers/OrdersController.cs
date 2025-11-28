using ApiFarmacia.DAL;
using ApiFarmacia.Dto;
using ApiFarmacia.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiFarmacia.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly Context _context;
    private readonly IMapper _mapper;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(Context context, IMapper mapper, ILogger<OrdersController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (dto.UsuarioId != usuarioId)
            return BadRequest(new { mensaje = "No puedes crear órdenes para otros usuarios" });

        var order = new Order
        {
            UsuarioId = dto.UsuarioId,
            Total = dto.Total,
            Estado = "COMPLETADO",
            MetodoPago = "PayPal",
            PayPalOrderId = dto.PaypalOrderId,
            PayPalPayerId = dto.PaypalPayerId,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var productoDto in dto.Productos)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                ProductoId = productoDto.ProductoId,
                Nombre = productoDto.Nombre,
                Cantidad = productoDto.Cantidad,
                Precio = productoDto.Precio
            };
            _context.OrderItems.Add(orderItem);
        }

        await _context.SaveChangesAsync();

        await _context.Entry(order).Collection(o => o.Items).LoadAsync();

        var response = _mapper.Map<OrderResponseDto>(order);

        _logger.LogInformation("Orden {OrderId} creada para usuario {UsuarioId}", order.OrderId, usuarioId);

        return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, new
        {
            mensaje = "Orden creada exitosamente",
            data = response
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
            return NotFound(new { mensaje = "Orden no encontrada" });

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rol = User.FindFirstValue(ClaimTypes.Role);

        if (order.UsuarioId != usuarioId && rol != "Administrador")
            return Forbid();

        var response = _mapper.Map<OrderResponseDto>(order);

        return Ok(new { mensaje = "Orden encontrada", data = response });
    }

   
    [HttpGet("usuario/{usuarioId}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserOrders(int usuarioId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rol = User.FindFirstValue(ClaimTypes.Role);

        // Solo el dueño o admin pueden ver las órdenes
        if (usuarioId != currentUserId && rol != "Administrador")
            return Forbid();

        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();

        var response = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

        return Ok(response);
    }

    
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound(new { mensaje = "Orden no encontrada" });

        order.Estado = dto.Estado;
        order.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Estado de orden {OrderId} actualizado a {Estado}", id, dto.Estado);

        return Ok(new { mensaje = "Estado actualizado exitosamente" });
    }
}