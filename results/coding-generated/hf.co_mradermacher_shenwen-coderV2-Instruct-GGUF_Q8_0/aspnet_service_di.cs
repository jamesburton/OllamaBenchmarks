using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailNotifier _emailNotifier;

        public OrdersController(IOrderRepository orderRepository, IEmailNotifier emailNotifier)
        {
            _orderRepository = orderRepository;
            _emailNotifier = emailNotifier;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Order order)
        {
            if (string.IsNullOrWhiteSpace(order.WarehouseCode))
                return BadRequest("Warehouse code is required");

            var result = await _orderRepository.CreateAsync(order);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Errors);

            await _emailNotifier.SendEmailAsync(
                new EmailMessage
                {
                    To = order.Email,
                    Subject = "Order Created",
                    Body = $"New order created: {order.Name}"
                });

            return CreatedAtAction(nameof(Get), new { id = result.Id }, order);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orderRepository.GetAsync(id);
            if (!order.Succeeded)
                return StatusCode(order.StatusCode, order.Errors);

            return Ok(order);
        }
    }
}