using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderService.Models;
using OrderService.Repositories;
using OrderService.Messaging;
using OrderService.DTOs;

namespace OrderService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepository _repo;
        private readonly RmqPublisher _publisher;

        public OrdersController(OrderRepository repo, RmqPublisher publisher)
        {
            _repo = repo;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var order = new Order
            {
                UserId = userId,
                Status = "Pending",
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var created = await _repo.Create(order);

            _publisher.Publish("order.placed", new
            {
                OrderId = created.OrderId,
                UserId = created.UserId,
                Items = dto.Items
            });

            return Ok(new { message = "Order placed, waiting for stock confirmation...", OrderId = created.OrderId });
        }

    }
}
