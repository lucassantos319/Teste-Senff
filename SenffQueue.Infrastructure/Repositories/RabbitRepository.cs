using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace SenffQueue.Infrastructure.Repositories
{
    public class RabbitRepository
    {
        private ConnectionFactory _factory;

        public RabbitRepository(string url = null)
        {
            if (string.IsNullOrEmpty(url))
                url = "localhost";

            if (_factory == null)
                _factory = new() { HostName = url };

        }

        public async Task SetQueue(string queueName)
        {
            using (var channel = await OpenConnectionAsync())
                await channel.QueueDeclareAsync(queue: queueName,
                                                durable: true, exclusive: false,
                                                autoDelete: false, arguments: null);

        }

        private async Task<List<string>> ReceiveMessage(string queueName,ushort prefetchCount = 100)
        {
            var messages = new List<string>();
            using (var channel = await OpenConnectionAsync())
            {
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    messages.Add(message);
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                };
            }
            
            return messages;
        }

        public async Task<IEnumerable<string>> GetMessages(string queueName)
        {
            try
            {
                var listMessages = await ReceiveMessage(queueName);
                return listMessages;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task SendMessage(string queueName, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return;

                var messageEncode = Encoding.UTF8.GetBytes(message);
                using (var channel = await OpenConnectionAsync())
                    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: messageEncode);
            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
        }

        private async Task<IChannel> OpenConnectionAsync()
        {
            try
            {
                var connection = await _factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                return channel; 
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}