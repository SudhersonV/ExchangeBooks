using System.Threading.Tasks;
using ExchangeBooks.Infra.Models.Domain;
using ExchangeBooks.Infra.Enums;
using System.Collections.Generic;
using System;

namespace ExchangeBooks.Infra.Interfaces
{
    public interface IMessagesService
    {
        #region Topics
        Task<Topic> AddTopic(Topic topic);
        Task<Topic> AddPostNamesToTopic(Guid topicId, List<string> postNames);
        Task<Topic> GetTopicByName(string name);
        Task<Topic> GetTopicById(Guid id);
        Task<List<Topic>> GetTopicsByTag(string searchTags);
        #endregion

        #region Subscriptions
        Task<Subscription> AddSubscription(Topic topic, string email);
        Task<Subscription> GetSubscriptionById(Guid id);
        Task<List<Subscription>> GetSubscriptionsByTopicId(Guid topicId);
        Task<List<Subscription>> GetActiveSubscriptionsByTopicId(Guid topicId);
        Task<Subscription> GetUserTopicSubscription(Topic topic, string email);
        Task<List<Subscription>> GetUserSubscriptions();
        Task<List<Subscription>> GetUserSubscriptionsByType(SubscriptionType type);
        Task<bool> CeaseSubscription(Topic topic);
        #endregion
        
        #region Messages
        Task<Message> AddMessage(Guid topicId, List<Guid> subscriptionIds, string title, string content);
        Task<List<Message>> GetMessagesBySubscriptionIds(List<Guid> subscriptionIds);
        Task<List<Message>> GetMessagesByTopicId(Guid topicId);
        Task AddSubscriptionsToMessage(Guid topicId, List<Guid> subscriptions);
        #endregion
    }
}