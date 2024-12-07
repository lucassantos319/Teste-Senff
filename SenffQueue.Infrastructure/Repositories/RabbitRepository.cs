using RabbitMQ.Client;
using System.Text;

namespace SenffQueue.Infrastructure.Repositories
{
    public class RabbitRepository
    {
        private ConnectionFactory _factory;

        public RabbitRepository(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("Url precisa ser informada corretamente.");

            if (_factory == null)
                _factory = new() { HostName = url };
        }

        public async Task SendMessage(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return;

                var messageEncode = Encoding.UTF8.GetBytes(message);
                using (var channel = await OpenConnectionAsync())
                    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "messages", body: messageEncode);
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
                return await connection.CreateChannelAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}