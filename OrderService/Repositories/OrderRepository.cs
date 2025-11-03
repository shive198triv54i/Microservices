using OrderService.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace OrderService.Repositories
{
    public class OrderRepository
    {
        private readonly OrderDbContext _ctx;
        public OrderRepository(OrderDbContext ctx) => _ctx = ctx;

        public async Task<Order> Get(int orderId)
        {
            return await _ctx.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

        }
        public async Task<Order> Create(Order order)
        {
            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync();
            return order;
        }

        public async Task Update(Order order)
        {
            _ctx.Orders.Update(order);
            await _ctx.SaveChangesAsync();
        }
    }
}
