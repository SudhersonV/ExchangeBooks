using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Interfaces.Repository;
using ExchangeBooks.Models;
using static ExchangeBooks.Constants.Constants;

namespace ExchangeBooks.Services.Http
{
    public class MessagesService : IMessagesService
    {
        private readonly IGenericRepository _repository;
        private readonly IAuthenticationService _authenticationService;

        public MessagesService(IGenericRepository repository, IAuthenticationService authenticationService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
        }

        public async Task<Topic> SubscribeToTopic(Enums.TopicType topicType, string[] tags)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var topic = await _repository.GetAsync<Topic>($@"{Api.Url}/{Api.Paths.Message.SubscribeToTopic
                    .Replace("{topicType}", ((int)topicType).ToString())
                    .Replace("{topicName}", string.Join(TagSeparator, tags))}", accessToken);
                return topic;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return null;
            }
        }

        public async Task<List<Topic>> UserTopics(Enums.TopicType topicType)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var response = await _repository.GetAsync<List<Topic>>($@"{Api.Url}/{Api.Paths.Message.UserTopics
                    .Replace("{topicType}", ((int)topicType).ToString())}", accessToken);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return new List<Topic>();
            }
        }

        public async Task<List<PushMessage>> GetMessages(Guid topicId)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var response = await _repository.GetAsync<List<PushMessage>>($@"{Api.Url}/{Api.Paths.Message.ChatMessages
                    .Replace("{topicId}", topicId.ToString())}", accessToken);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return new List<PushMessage>();
            }
        }
        public async Task<List<PushMessage>> GetNotifications()
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var response = await _repository.GetAsync<List<PushMessage>>($@"{Api.Url}/{Api.Paths.Message.Notifications}", accessToken);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return new List<PushMessage>();
            }
        }

        public async Task SendMessage(Guid topicId, string message)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.PostAsync($@"{Api.Url}/{Api.Paths.Message.SendMessage
                    .Replace("{topicId}", topicId.ToString())}", message, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task UnsubscribeTopic(Guid id)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                await _repository.PatchAsync($@"{Api.Url}/{Api.Paths.Message.UnsubscribeToTopic
                    .Replace("{id}", id.ToString())}", string.Empty, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task<Topic> GetTopic(Guid id)
        {
            var accessToken = await _authenticationService.GetAccessToken();
            try
            {
                var topic = await _repository.GetAsync<Topic>($@"{Api.Url}/{Api.Paths.Message.GetTopic
                    .Replace("{id}", id.ToString())}", accessToken);
                return topic;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                return null;
            }
        }
    }
}
