using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Repositories;

namespace OrderService.Messaging
{
    public class RmqConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IModel _ch;

        public RmqConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory { HostName = "localhost" };
            var conn = factory.CreateConnection();
            _ch = conn.CreateModel();

            _ch.ExchangeDeclare("product.exchange", ExchangeType.Direct, durable: true);
            _ch.QueueDeclare("order.result", durable: true, exclusive: false, autoDelete: false);
            _ch.QueueBind("order.result", "product.exchange", "product.stock.result");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_ch);

            consumer.Received += async (sender, e) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<OrderRepository>();

                try
                {
                    var json = Encoding.UTF8.GetString(e.Body.Span);
                    var msg = JsonSerializer.Deserialize<OrderResultMessage>(json);

                    var order = await repo.Get(msg.OrderId);
                    order.Status = msg.IsSuccess ? "Completed" : "Failed";

                    await repo.Update(order);

                    _ch.BasicAck(e.DeliveryTag, false);
                }
                catch
                {
                    _ch.BasicNack(e.DeliveryTag, false, true); // requeue message on failure
                }
            };

            _ch.BasicConsume("order.result", false, consumer);
            return Task.CompletedTask;
        }
    }

    public record OrderResultMessage(int OrderId, bool IsSuccess);
}
