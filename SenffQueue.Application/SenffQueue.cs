using SenffQueue.Domain.Interfaces.Application;
using SenffQueue.Infrastructure.Repositories;

namespace SenffQueue.Application
{
    public class SenffQueue : IApplication
    {
        public RabbitRepository _repository;

        public SenffQueue(string queueName)
        {
            if ( _repository == null ) 
                _repository = new(queueName);
        }
        public async Task CreateQueue(string queueName)
        {
            try
            {
                if (string.IsNullOrEmpty(queueName))
                    throw new Exception("Error: queue name should be different than null or empty");

                await _repository.SetQueue(queueName);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<string>> ReceiveMessage(string queueName = null)
        {
            try
            {
                var queueMessages = await _repository.GetMessages(queueName);
                if (queueMessages == null || !queueMessages.Any())
                    return null;

                return queueMessages;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> SendMessage(string message, string queueName = null)
        {
            try
            {
                await _repository.SendMessage(message, queueName);
                return true;
            }
            catch 
            {
                throw;
            }
        }
    }
}