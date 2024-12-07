using SenffQueue.Infrastructure.Repositories;

internal static class Program
{
    private static async Task Main()
    {
        var rabbitMq = new RabbitRepository("localhost");
        await rabbitMq.SendMessage("Teste");
    }
}