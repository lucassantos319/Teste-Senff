using SenffQueue.Infrastructure.Repositories;

var rabbitMq = new RabbitRepository("localhost");
using var connection = rabbitMq.