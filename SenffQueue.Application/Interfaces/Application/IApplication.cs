namespace SenffQueue.Domain.Interfaces.Application
{
    public interface IApplication
    {
        public Task<IEnumerable<string>> ReceiveMessage(string queueName = null);

        public Task<bool> SendMessage(string message,string queueName = null);
        public Task CreateQueue(string queueName);
    }
}