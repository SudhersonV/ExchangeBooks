using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeBooks.Models;

namespace ExchangeBooks.Interfaces.Http
{
    public interface IMessagesService
    {
        Task<Topic> SubscribeToTopic(Enums.TopicType topicType, string[] tags);
        Task<List<Topic>> UserTopics(Enums.TopicType topicType);
        Task UnsubscribeTopic(Guid id);
        Task<List<PushMessage>> GetMessages(Guid topicId);
        Task<List<PushMessage>> GetNotifications();
        Task SendMessage(Guid topicId, string message);
        Task<Topic> GetTopic(Guid id);
    }
}
