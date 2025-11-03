using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderService.Messaging
{
    public class RmqPublisher
    {
        private readonly IConnection _conn;
        private readonly IModel _ch;

        public RmqPublisher()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _conn = factory.CreateConnection();
            _ch = _conn.CreateModel();

            _ch.ExchangeDeclare("order.exchange", ExchangeType.Direct, durable: true);

            // ✅ ensure queue exists even if Product Service is DOWN
            _ch.QueueDeclare("order.placed", durable: true, exclusive: false, autoDelete: false);
            _ch.QueueBind("order.placed", "order.exchange", "order.placed");
        }

        public void Publish(string routingKey, object message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var props = _ch.CreateBasicProperties();
            props.Persistent = true; // ✅ make message survive restart

            _ch.BasicPublish(
                exchange: "order.exchange",
                routingKey: routingKey,
                basicProperties: props,
                body: body
            );
        }
    }
}
