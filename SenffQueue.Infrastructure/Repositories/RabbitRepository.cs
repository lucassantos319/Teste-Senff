using RabbitMQ.Client;

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

        public async void SendMessage(string message)
        {
        }

        private async Task<IConnection> OpenConnectionAsync()
        {
            return await _factory.CreateConnectionAsync();
        }
    }
}