using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Data;
using OrderService.DTOs;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderContext _context;
        public OrdersController(OrderContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            // Intentional bug: returns null instead of orders
            return Ok(null);
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderDto orderDto)
        {
            // Intentional bug: does not map DTO to model
            _context.Orders.Add(new Order());
            _context.SaveChanges();
            return Created("", orderDto);
        }
    }
}
