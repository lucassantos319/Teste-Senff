﻿using SenffQueue.Domain.Interfaces.Application;
using SenffQueue.Infrastructure.Repositories;
using SenffQueue.Interfaces.Repositories;

namespace SenffQueue
{
    public class SenffQueue : IApplication
    {
        //<summary> Repository that's communicate with rabbitmq allowing easier integration</summary>
        private static IRabbitRepository _repository;

        public static async Task<SenffQueue> BuildQueue(string queueName,string url = "localhost",int qtyFetch = 100)
        {
            var repository = await RabbitRepository.BuildRepository(queueName,url, qtyFetch);
            return new SenffQueue(repository,queueName);
        }

        //<summary>Constructor of the application</summary>
        //<param name="queueName">Queue name that will be use by add and receive messages</param>
        private SenffQueue(IRabbitRepository repository, string queueName)
        {
            _repository = repository;
        }

        //<summary>Create a new queue</summary>
        //<param name="queueName">Queue that will be created</param>
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

        //<summary>Function that's allow the receive of messages from a certain queue or default queue set in constructor</summary>
        //<param name="queueName"> Specify certain queue to get the messages</param>
        //<return>Return a range of 100 messages of the queue</return>
        public async Task<IEnumerable<string>> ReceiveMessage(string queueName = null, ushort prefetchCount = 100)
        {
            try
            {
                var queueMessages = await _repository.GetMessages(queueName,prefetchCount);
                if (queueMessages == null || !queueMessages.Any())
                    return null;

                return queueMessages;
            }
            catch
            {
                throw;
            }
        }

        //<summary>Function that's send a messages from a certain queue or the default queue</summary>
        //<param name="message">Message to be send</param>
        //<param name="queueName"> Specify certain queue to send the messages</param>
        //<return>Return true if send the message</return>
        public async Task<bool> SendMessage(string message, string queueName = null)
        {
            try
            {
                return await _repository.SendMessage(message, queueName);
            }
            catch 
            {
                throw;
            }
        }
    }
}