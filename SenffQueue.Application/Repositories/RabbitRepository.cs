using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using SenffQueue.Interfaces.Repositories;
using System.Text;
using System.Threading.Channels;

namespace SenffQueue.Infrastructure.Repositories
{
    internal class RabbitRepository : IRabbitRepository
    {
        private static string _queueName;
        private static IChannel _channel;
        private static ConnectionFactory _factory;
        private static readonly int limitAttempts = 5;
        private static int reSendMessageAttempts = 1;
        private static int retryConnectionAttempts = 1;

        public static async Task<RabbitRepository> BuildRepository(string queueName, string url = "localhost")
        {
            var factory = new ConnectionFactory() { HostName = url };
            var channel = await OpenConnectionAsync(factory);

            return new RabbitRepository(channel,factory,queueName,url);
        }
        
        //<summary>Constructor of the repository</summary>
        //<param name="queueName">Queue name to use as default</param>
        private RabbitRepository(IChannel channel, ConnectionFactory factory, string queueName, string url = null)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException("Queue name não preenchido");

            if (string.IsNullOrEmpty(url))
                url = "localhost";
            
            if (channel == null)
                throw new Exception("Erro ao abrir conexão com o rabbitmq");

            _factory = factory;
            _queueName = queueName;
            _channel = channel;
        }


        //<summary>Set a new queue into rabbitmq</summary>
        //<param name="queueName">Name of the new queue</param>
        public async Task SetQueue(string queueName)
        {
            try
            {
                await _channel.QueueDeclareAsync(queue: queueName ?? _queueName,
                                                durable: true, exclusive: false,
                                                autoDelete: false, arguments: null);
            }
            catch(AlreadyClosedException)
            {
                _channel = await OpenConnectionAsync();
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
          
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: prefetchCount, global: false);
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                messages.Add(message);
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            };
                
            await _channel.BasicConsumeAsync(queueName ?? _queueName, autoAck: false, consumer: consumer);
            await Task.Delay(5000);
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
            catch (AlreadyClosedException ex)
            {
                _channel = await OpenConnectionAsync();
                return await GetMessages();
            }
            catch (OperationInterruptedException)
            {
                await SetQueue(_queueName);
                return Enumerable.Empty<string>();
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
                await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName ?? _queueName, body: messageEncode);
                return true;
            }
            catch (OperationInterruptedException)
            {
                await SetQueue(_queueName);
                if ( reSendMessageAttempts <= limitAttempts )
                {
                    reSendMessageAttempts++;
                    return await SendMessage(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error send message: {ex.ToString()}");
                return false;
            }

            reSendMessageAttempts = 0;
            return false;
        }

        //<summary>Function to make the connection with rabbitmq</summary>
        private static async Task<IChannel> OpenConnectionAsync(ConnectionFactory factory = null)
        {
            if (retryConnectionAttempts <= limitAttempts)
            {
                try
                {
                    var hostname = _factory != null ? _factory.HostName : factory.HostName;
                    var connection =  _factory != null ? await _factory.CreateConnectionAsync() : await factory.CreateConnectionAsync();
                    var channel = await connection.CreateChannelAsync();

                    Console.WriteLine($"Conexão com o rabbitmq estabelecida => url: {hostname}");
                    return channel; 
                }
                catch (BrokerUnreachableException)
                {
                    Console.WriteLine($"Tentando conectar com o rabbitmq... {retryConnectionAttempts}");
                    await Task.Delay(8000);
                    retryConnectionAttempts++;
                    return await OpenConnectionAsync(factory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error open connection rabbitmq: {ex.ToString()}");
                    return null;
                }
            }

            retryConnectionAttempts = 0;
            return null;
        }
    }
}