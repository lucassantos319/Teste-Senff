namespace SenffQueue.Domain.Interfaces.Application
{
    public interface IApplication
    {
        public void ListingQueue();

        public Task<string> ReceiveMessage();

        public Task<bool> SendMessage(string message);
    }
}