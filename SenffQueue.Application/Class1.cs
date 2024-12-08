using SenffQueue.Domain.Interfaces.Application;

namespace SenffQueue.Application
{
    public class SenffQueue : IApplication
    {
        public SenffQueue()
        { }

        public void ListingQueue()
        {
            throw new NotImplementedException();
        }

        public Task<string> ReceiveMessage()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendMessage(string message)
        {
            throw new NotImplementedException();
        }
    }
}