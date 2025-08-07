using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ExchangeBooks.Infra;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Interfaces;
using ExchangeBooks.Infra.Models.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static ExchangeBooks.Infra.Constants;


namespace ExchangeBooks.Services
{
    public class MessagesService : IMessagesService
    {
        #region Variables
        private Container _topicContainer, _subscriptionContainer, _messageContainer;
        private readonly IClaimsAccessor _claimsAccessor;
        private readonly CosmosDbOptions _cosmosDbOptions;
        private readonly IFcmService _fcmService;
        #endregion

        #region Constructor
        public MessagesService(CosmosClient dbClient, IClaimsAccessor claimsAccessor
        , IOptions<CosmosDbOptions> cosmosDbOptions, IFcmService fcmService)
        {
            _cosmosDbOptions = cosmosDbOptions.Value;
            _claimsAccessor = claimsAccessor;
            _fcmService = fcmService;
            _topicContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.TopicsContainerId);
            _subscriptionContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.SubscriptionsContainerId);
            _messageContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.MessagesContainerId);
        }
        #endregion

        #region Public Methods
        #region Topics
        public async Task<Topic> AddTopic(Topic topic)
        {
            try
            {
                var existingTopic = await GetTopicByName(topic.Name);
                if (existingTopic != null)
                    return existingTopic;
                topic.Id = Guid.NewGuid();
                topic.FcmId = _fcmService.GenerateFcmTopicId(topic.Name, topic.Id);
                var response = await _topicContainer.CreateItemAsync(topic);
                return response.StatusCode == HttpStatusCode.Created ?
                response.Resource : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Topic> AddPostNamesToTopic(Guid topicId, List<string> postNames)
        {
            var topic = await GetTopicById(topicId);
            var newPostNames = topic.PostNames;
            newPostNames.AddRange(postNames);
            topic.PostNames = newPostNames;
            return await _topicContainer.ReplaceItemAsync<Topic>(topic, topic.Id.ToString());
        }
        public async Task<Topic> GetTopicByName(string name)
        {
            try
            {
                var iterator = _topicContainer.GetItemLinqQueryable<Topic>(true)
                .Where(t => t.Name.ToLower() == name.ToLower()).Take(1).ToFeedIterator();
                var topic = await iterator.ReadNextAsync();
                return topic.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Topic> GetTopicById(Guid id)
        {
            try
            {
                var iterator = _topicContainer.GetItemLinqQueryable<Topic>(true)
                .Where(t => t.Id == id).Take(1).ToFeedIterator();
                var topics = await iterator.ReadNextAsync();
                return topics.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Topic>> GetTopicsByTag(string searchTags)
        {
            var topics = new List<Topic>();
            try
            {
                var response = await _topicContainer.Scripts.ExecuteStoredProcedureAsync<string>("getTopicsByTag",
                new PartitionKey(string.Empty), new dynamic[] { searchTags });
                topics = JsonConvert.DeserializeObject<List<Topic>>(response.Resource);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return topics;
        }
        #endregion

        #region Subscriptions
        public async Task<Subscription> AddSubscription(Topic topic, string email)
        {
            try
            {
                var existingSubscription = await GetUserTopicSubscription(topic, email);
                if (existingSubscription == null)
                {
                    var subscription = new Subscription
                    {
                        Id = Guid.NewGuid(),
                        TopicId = topic.Id,
                        Type = TopicToSubscriptionMapping[topic.Type],
                        Name = $"{topic.Name}_{topic.Type.ToString()}_{TopicToSubscriptionMapping[topic.Type].ToString()}",
                        Email = email,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        ModifiedOn = DateTime.UtcNow
                    };
                    return await _subscriptionContainer.CreateItemAsync(subscription);
                }
                else if (!existingSubscription.IsActive)
                {
                    await AlterUserSubscriptionStatus(topic, email, true);
                    return existingSubscription;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Subscription> GetSubscriptionById(Guid id)
        {
            try
            {
                var iterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true)
                .Where(s => s.Id == id).Take(1).ToFeedIterator();
                var subscriptions = await iterator.ReadNextAsync();
                return subscriptions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Subscription>> GetSubscriptionsByTopicId(Guid topicId)
        {
            var subscriptions = new List<Subscription>();
            try
            {
                var iterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true)
                    .Where(s => s.TopicId == topicId).ToFeedIterator();
                var entities = await iterator.ReadNextAsync();
                subscriptions = entities.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptions;
        }
        public async Task<List<Subscription>> GetActiveSubscriptionsByTopicId(Guid topicId)
        {
            var subscriptions = new List<Subscription>();
            try
            {
                var iterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true)
                    .Where(s => s.TopicId == topicId
                    && s.IsActive).ToFeedIterator();
                var entities = await iterator.ReadNextAsync();
                subscriptions = entities.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptions;
        }
        public async Task<Subscription> GetUserTopicSubscription(Topic topic, string email)
        {
            try
            {
                var iterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true)
                .Where(s => s.TopicId == topic.Id
                    && s.Type == TopicToSubscriptionMapping[topic.Type]
                    && s.Email == email)
                    .Take(1).ToFeedIterator();
                var subscriptions = await iterator.ReadNextAsync();
                return subscriptions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // public async Task<string> GetUserFcmToken()
        // {
        //     try
        //     {
        //         var iterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true)
        //         .Where(s => s.Email == _claimsAccessor.Email).Take(1).ToFeedIterator();
        //         var subscriptions = await iterator.ReadNextAsync();
        //         return subscriptions.FirstOrDefault()?.Token;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw ex;
        //     }
        // }
        public async Task<List<Subscription>> GetUserSubscriptions()
        {
            var subscriptions = new List<Subscription>();
            try
            {
                var iterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true)
                    .Where(s => s.Email == _claimsAccessor.Email).ToFeedIterator();
                var entities = await iterator.ReadNextAsync();
                subscriptions = entities.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptions;
        }
        public async Task<List<Subscription>> GetUserSubscriptionsByType(SubscriptionType type)
        {
            var subscriptions = new List<Subscription>();
            try
            {
                var iterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true)
                    .Where(s => s.Email == _claimsAccessor.Email
                    && s.Type == type && s.IsActive).ToFeedIterator();
                var entities = await iterator.ReadNextAsync();
                subscriptions = entities.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptions;
        }
        
        public async Task<bool> CeaseSubscription(Topic topic)
        {
            return await AlterUserSubscriptionStatus(topic, _claimsAccessor.Email, false);
        }
        #endregion

        #region Messages
        public async Task<Message> AddMessage(Guid topicId, List<Guid> subscriptionIds, string title, string content)
        {
            try
            {
                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    TopicId = topicId,
                    SubscriptionIds = subscriptionIds,
                    Title = title,
                    Content = content,
                    CreatedBy = _claimsAccessor.Email,
                    CreatedOn = DateTime.UtcNow
                };
                var response = await _messageContainer.CreateItemAsync(message);
                return response.StatusCode == HttpStatusCode.Created ?
                    response.Resource : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Message>> GetMessagesBySubscriptionIds(List<Guid> subscriptionIds)
        {
            var messages = new List<Message>();
            try
            {
                var response = await _messageContainer.Scripts.ExecuteStoredProcedureAsync<string>("getMessagesBySubscriptionIds",
                new PartitionKey(string.Empty), new dynamic[] { subscriptionIds });
                messages = JsonConvert.DeserializeObject<List<Message>>(response.Resource);
                return messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Message>> GetMessagesByTopicId(Guid topicId)
        {
            try
            {
                var iterator = _messageContainer.GetItemLinqQueryable<Message>(true)
                .Where(m => m.TopicId == topicId).ToFeedIterator();
                var messages = await iterator.ReadNextAsync();
                return messages.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task AddSubscriptionsToMessage(Guid topicId, List<Guid> subscriptions)
        {
            var storeMessages = await GetMessagesByTopicId(topicId);
            await storeMessages.ForEachAsync(async sm =>
            {
                var newSubscriptions = sm.SubscriptionIds.ToList();
                newSubscriptions.AddRange(subscriptions);
                sm.SubscriptionIds = newSubscriptions;
                await _messageContainer.ReplaceItemAsync<Message>(sm, sm.Id.ToString());
            });
        }
        #endregion
        #endregion
        
        #region Private Methods
        private async Task<bool> AlterUserSubscriptionStatus(Topic topic, string email, bool status)
        {
            try
            {
                var existingSubscription = await GetUserTopicSubscription(topic, email);
                if (existingSubscription != null || existingSubscription.IsActive != status)
                {
                    existingSubscription.IsActive = status;
                    existingSubscription.ModifiedOn = DateTime.UtcNow;
                    await _subscriptionContainer.ReplaceItemAsync<Subscription>(existingSubscription, existingSubscription.Id.ToString());
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}