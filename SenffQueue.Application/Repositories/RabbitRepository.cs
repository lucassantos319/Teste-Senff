using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SenffQueue.Interfaces.Repositories;
using System.Text;
using System.Threading.Channels;

namespace SenffQueue.Infrastructure.Repositories
{
    public class RabbitRepository : IRabbitRepository
    {
        private ConnectionFactory _factory;
        private string _queueName;

        //<summary>Constructor of the repository</summary>
        //<param name="queueName">Queue name to use as default</param>
        public RabbitRepository(string queueName, string url = null)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException("Queue name não preenchido");

            _queueName = queueName;
            if (string.IsNullOrEmpty(url))
                url = "localhost";

            if (_factory == null)
                _factory = new() { HostName = url };

        }


        //<summary>Set a new queue into rabbitmq</summary>
        //<param name="queueName">Name of the new queue</param>
        public async Task SetQueue(string queueName)
        {
            try
            {
                using (var channel = await OpenConnectionAsync())
                {
                    if (channel == null)
                        throw new Exception("Erro ao abrir conexão com o rabbitmq");

                    await channel.QueueDeclareAsync(queue: queueName ?? _queueName,
                                                    durable: true, exclusive: false,
                                                    autoDelete: false, arguments: null);
                }
            }
            catch
            {
                throw;
            }
        }

        //<summary>Function to get messages in the queue</summary>
        //<param name="queueName">Queue selected to get the messages</param>
        //<return>List of the messages that is in the queue</return>
        private async Task<List<string>> ReceiveMessage(string queueName = null ,ushort prefetchCount = 100)
        {
            var messages = new List<string>();
            using (var channel = await OpenConnectionAsync())
            {
                if (channel == null)
                    throw new Exception("Erro ao abrir conexão com o rabbitmq");

                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: prefetchCount, global: false);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    messages.Add(message);
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                
                await channel.BasicConsumeAsync(queueName ?? _queueName, autoAck: false, consumer: consumer);
            }

            return messages;
        }


        //<summary>Function to get messages in the queue</summary>
        //<param name="queueName">Queue selected to get the messages</param>
        //<return>List of the messages that is in the queue</return>
        public async Task<IEnumerable<string>> GetMessages(string queueName = null)
        {
            try
            {
                var listMessages = await ReceiveMessage(queueName);
                return listMessages;
            }
            catch
            {
                throw;
            }
        }


        //<summary>Function to send messages in the queue</summary>
        //<param name="message">Message to send</param>
        //<param name="queueName">Queue selected to send the messages</param>
        //<return>Inform if the operation of sending is done</return>
        public async Task<bool> SendMessage(string message,string queueName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return false;

                var messageEncode = Encoding.UTF8.GetBytes(message);
                using (var channel = await OpenConnectionAsync())
                {
                    if (channel == null)
                        throw new Exception("Erro ao abrir conexão com o rabbitmq");

                    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName ?? _queueName, body: messageEncode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error send message: {ex.ToString()}");
                return false;
            }

            return false;
        }

        //<summary>Function to make the connection with rabbitmq</summary>
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
                Console.WriteLine($"Error open connection rabbitmq: {ex.ToString()}");
                return null;
            }
        }
    }
}