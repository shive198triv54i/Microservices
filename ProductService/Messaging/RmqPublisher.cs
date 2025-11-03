using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ProductService.Messaging
{
    public class RmqPublisher
    {
        private readonly IModel _ch;

        public RmqPublisher()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            var conn = factory.CreateConnection();
            _ch = conn.CreateModel();

            _ch.ExchangeDeclare("product.exchange", ExchangeType.Direct, durable: true);

            // ✅ ensure queue exists even if Order Service is DOWN
            _ch.QueueDeclare("order.result", durable: true, exclusive: false, autoDelete: false);
            _ch.QueueBind("order.result", "product.exchange", "product.stock.result");
        }

        public void Publish(string routingKey, object msg)
        {
            var json = JsonSerializer.Serialize(msg);
            var body = Encoding.UTF8.GetBytes(json);

            var props = _ch.CreateBasicProperties();
            props.Persistent = true;

            _ch.BasicPublish(
                exchange: "product.exchange",
                routingKey: routingKey,
                basicProperties: props,
                body: body
            );
        }
    }
}
