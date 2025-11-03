using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Repositries;

namespace ProductService.Messaging
{
    public class OrderPlacedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RmqPublisher _publisher;
        private readonly IModel _ch;

        public OrderPlacedConsumer(IServiceScopeFactory scopeFactory, RmqPublisher publisher)
        {
            _scopeFactory = scopeFactory;
            _publisher = publisher;

            var factory = new ConnectionFactory { HostName = "localhost" };
            var conn = factory.CreateConnection();
            _ch = conn.CreateModel();

            _ch.ExchangeDeclare("order.exchange", ExchangeType.Direct, durable: true);
            _ch.QueueDeclare("order.placed", durable: true, exclusive: false,autoDelete:false);
            _ch.QueueBind("order.placed", "order.exchange", "order.placed");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_ch);

            consumer.Received += async (sender, e) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ProductRepository>();

                try
                {
                    var json = Encoding.UTF8.GetString(e.Body.Span);
                    var msg = JsonSerializer.Deserialize<OrderPlacedMessage>(json);

                    bool success = true;

                    foreach (var item in msg.Items)
                    {
                        var product = await repo.Get(item.ProductId);
                        if (product == null || product.Stock < item.Quantity)
                        {
                            success = false;
                            break;
                        }
                    }

                    if (success)
                    {
                        foreach (var item in msg.Items)
                        {
                            var product = await repo.Get(item.ProductId);
                            product.Stock -= item.Quantity;
                            await repo.Update(product);
                        }
                    }

                    _publisher.Publish("product.stock.result", new { OrderId = msg.OrderId, IsSuccess = success });

                    _ch.BasicAck(e.DeliveryTag, false);
                }
                catch
                {
                    _ch.BasicNack(e.DeliveryTag, false, true);
                }
            };

            _ch.BasicConsume("order.placed", false, consumer);
            return Task.CompletedTask;
        }
    }

    public class OrderPlacedMessage
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public List<ItemMessage> Items { get; set; }
    }

    public class ItemMessage
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
