using RabbitMQ.Client;
using System.Text;
using UserService.Models;

namespace UserService.Messaging
{
    public class RabbitMqPublisher
    {
        private readonly IConfiguration _configuration;
        public RabbitMqPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishUserRegistered(string message)
        {
            var factory = new ConnectionFactory {
              Uri = new Uri(_configuration["RabbitMQ:ConnectionString"])
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            channel.QueueDeclareAsync("user.registered", durable: true, exclusive:false);
            var properties = new BasicProperties
            {
                Persistent = true
            };

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: "",routingKey: "user.registered",
                mandatory: false,basicProperties: properties,body: body);
        }
    }
}
