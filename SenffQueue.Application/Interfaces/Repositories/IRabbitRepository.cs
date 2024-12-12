

namespace SenffQueue.Interfaces.Repositories
{
    internal interface IRabbitRepository
    {
        public Task SetQueue(string queueName);
        public Task<IEnumerable<string>> GetMessages(string queueName = null,ushort prefetchCount = 100);
        public Task<bool> SendMessage(string message, string queueName = null);
    }
}
