using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeBooks.Infra;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Interfaces;
using ExchangeBooks.Infra.Models.Domain;
using ExchangeBooks.Infra.Models.Response;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static ExchangeBooks.Infra.Constants;
using FcmMessage = FirebaseAdmin.Messaging.Message;

namespace ExchangeBooks.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IMessagesService _topicService;
        private readonly IClaimsAccessor _claimsAccessor;
        private readonly IPostsService _postService;
        private readonly IFcmService _fcmService;
        public MessagesController(ILogger<MessagesController> logger, IMessagesService topicService
        , IClaimsAccessor claimsAccessor, IPostsService postService, IFcmService fcmService)
        {
            _logger = logger;
            _topicService = topicService;
            _claimsAccessor = claimsAccessor;
            _postService = postService;
            _fcmService = fcmService;
        }

        // Test Method
        [AllowAnonymous]
        [HttpPost("[action]/{topicId}")]
        public async Task<IActionResult> SendTopicMessage(string topicId, PushMessage message)
        {
            FcmMessage pushMessage;
            if (message.Type != SubscriptionType.Push) return BadRequest();
            pushMessage = new FcmMessage
            {
                Data = new Dictionary<string, string>
                        {
                            { Constants.PushMessages.MessageId, $"{message.MessageId}" },
                            { Constants.PushMessages.TopicId, $"{message.TopicId}" },
                            { Constants.PushMessages.Type, $"{message.Type}" },
                            { Constants.PushMessages.Title, message.Title },
                            { Constants.PushMessages.Content, message.Content },
                            { Constants.PushMessages.CreatedBy, message.CreatedBy },
                            { Constants.PushMessages.CreatedOn, $"{message.CreatedOn}" }
                        },
                Topic = topicId,
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = message.Title,
                            Body = message.Content
                        }
                    }
                }
            };
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(pushMessage);
            Console.WriteLine("Successfully sent message: " + response);
            return Ok(response);
        }

        // Test Method
        [AllowAnonymous]
        [HttpPost("[action]/{registrationToken}")]
        public async Task<IActionResult> SendMessage(string registrationToken, PushMessage message)
        {
            FcmMessage pushMessage;
            if (message.Type == SubscriptionType.Push)
            {
                pushMessage = new FcmMessage
                {
                    Data = new Dictionary<string, string>
                        {
                            { Constants.PushMessages.MessageId, $"{message.MessageId}" },
                            { Constants.PushMessages.TopicId, $"{message.TopicId}" },
                            { Constants.PushMessages.Type, $"{message.Type}" },
                            { Constants.PushMessages.Title, message.Title },
                            { Constants.PushMessages.Content, message.Content },
                            { Constants.PushMessages.CreatedBy, message.CreatedBy },
                            { Constants.PushMessages.CreatedOn, $"{message.CreatedOn}" }
                        },
                    Token = registrationToken,
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Alert = new ApsAlert
                            {
                                Title = message.Title,
                                Body = message.Content
                            }
                        }
                    }
                };
            }
            else
            {
                pushMessage = new FcmMessage
                {
                    Data = new Dictionary<string, string>
                        {
                            { Constants.PushMessages.MessageId, $"{message.MessageId}" },
                            { Constants.PushMessages.TopicId, $"{message.TopicId}" },
                            { Constants.PushMessages.Type, $"{message.Type}" },
                            { Constants.PushMessages.Title, message.Title },
                            { Constants.PushMessages.Content, message.Content },
                            { Constants.PushMessages.CreatedBy, message.CreatedBy },
                            { Constants.PushMessages.CreatedOn, $"{message.CreatedOn}" }
                        },
                    Token = registrationToken
                };
            }
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(pushMessage);
            Console.WriteLine("Successfully sent message: " + response);
            return Ok(response);
        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]/{id:Guid}")]
        public async Task<IActionResult> Topic(Guid id)
        {
            var topic = await _topicService.GetTopicById(id);
            if (topic is null)
                return NotFound();

            return Ok(topic);
        }


        [Authorize("checkwritescope")]
        [HttpGet("[action]/{topicType:int}/{topicName}")]
        public async Task<IActionResult> SubscribeToTopic(int topicType, string topicName)
        {
            #region Validation
            Post subscribingChatPost = null;
            switch (topicType)
            {
                case (int)TopicType.Chat:
                    var postId = topicName.Split(Constants.TagSeparator)[1];
                    subscribingChatPost = await _postService.GetPost(new Guid(postId));
                    if (subscribingChatPost is null
                        || subscribingChatPost.CreatedBy == _claimsAccessor.Email)
                        return BadRequest($"Invalid post: {postId}");
                    break;
                default:
                    break;
            }
            #endregion
            var topic = new Topic
            {
                Name = topicName,
                Type = (TopicType)topicType,
                CreatedBy = _claimsAccessor.Email,
                CreatedOn = DateTime.UtcNow
            };
            //Add or get topic
            var storeTopic = await _topicService.AddTopic(topic);
            if (storeTopic is null)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Cannot add topic: {topic.Name}");
            var storeSubscription = await _topicService.GetUserTopicSubscription(storeTopic, _claimsAccessor.Email);
            if (storeSubscription != null)
                return Ok(storeTopic);
            storeSubscription = await _topicService.AddSubscription(storeTopic, _claimsAccessor.Email);
            if (storeSubscription is null)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Cannot add subscription for topic: {topic.Name}");
            var mytoken = await _fcmService.GetToken();
            await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(new List<string> { mytoken }, storeTopic.FcmId);
            await _topicService.AddSubscriptionsToMessage(storeTopic.Id, new List<Guid> { storeSubscription.Id });
            #region Add chat partner to subscriptions
            switch (topic.Type)
            {
                case TopicType.Chat:
                    await _topicService.AddPostNamesToTopic(topic.Id, new List<string> { subscribingChatPost?.Name });
                    storeSubscription = await _topicService.AddSubscription(storeTopic, subscribingChatPost?.CreatedBy);
                    if (storeSubscription != null)
                    {
                        var partnerToken = await _fcmService.GetToken(subscribingChatPost.CreatedBy);
                        await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(new List<string> { partnerToken }, storeTopic.FcmId);
                    }
                    break;
                default:
                    break;
            }
            #endregion
            return Ok(storeTopic);
        }

        [Authorize("checkwritescope")]
        [HttpPost("[action]/{topicId:Guid}")]
        public async Task<IActionResult> Message(Guid topicId, [FromBody] string content)
        {
            if (string.IsNullOrEmpty(content)) return BadRequest();
            var topic = await _topicService.GetTopicById(topicId);
            if (topic is null || topic.Type != TopicType.Chat)
                return BadRequest($"Invalid topic for chat: {topic.Name}");
            var subscriptions = await _topicService.GetActiveSubscriptionsByTopicId(topic.Id);
            if (!subscriptions.Any())
                return BadRequest($"Cannot add message when subscriptions are absent for chat topic: {topic.Name}");
            var selfSubscriber = subscriptions.FirstOrDefault(s => s.Email == _claimsAccessor.Email);
            var otherSubscribers = subscriptions.Where(s => s.Email != _claimsAccessor.Email).ToList();
            if (!otherSubscribers.Any()) return Ok();
            var message = await _topicService.AddMessage(topic.Id, otherSubscribers.Select(ss => ss.Id).ToList(), TopicToSubscriptionMapping[topic.Type].ToString(), content);
            var tokens = new List<string>();
            await otherSubscribers.ToList().ForEachAsync(async (s) =>
            {
                var token = await _fcmService.GetToken(s.Email);
                tokens.Add(token);
            });
            var multicatsMessage = new MulticastMessage
            {
                Tokens = tokens,
                Data = new Dictionary<string, string>
                        {
                            { Constants.PushMessages.MessageId, $"{message.Id}" },
                            { Constants.PushMessages.TopicId, $"{message.TopicId}" },
                            { Constants.PushMessages.Type, $"{TopicToSubscriptionMapping[topic.Type]}" },
                            { Constants.PushMessages.Title, TopicToSubscriptionMapping[topic.Type].ToString() },
                            { Constants.PushMessages.Content, message.Content },
                            { Constants.PushMessages.CreatedBy, message.CreatedBy },
                            { Constants.PushMessages.CreatedOn, $"{message.CreatedOn}" }
                        },
            };
            await FirebaseMessaging.DefaultInstance.SendMulticastAsync(multicatsMessage);
            return Ok();
        }

        [Authorize("checkwritescope")]
        [HttpPatch("[action]/{id}")]
        public async Task<IActionResult> UnsubscribeTopic(Guid id)
        {
            var storeTopic = await _topicService.GetTopicById(id);
            if (storeTopic == null)
                return BadRequest("Invalid topic id");
            if (!await _topicService.CeaseSubscription(storeTopic))
                return BadRequest($"No subscription found for topic id: {id}");
            var token = await _fcmService.GetToken();
            await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(new List<string> { token }, storeTopic.FcmId);
            return Ok();
        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]/{topicType:int}")]
        public async Task<IActionResult> Topics(TopicType topicType)
        {
            var topics = new List<Topic>();
            var subscriptions = await _topicService.GetUserSubscriptionsByType(TopicToSubscriptionMapping[topicType]);
            await subscriptions.ForEachAsync(async sub =>
            {
                var topic = await _topicService.GetTopicById(sub.TopicId);
                topics.Add(topic);
            });
            return Ok(topics);
        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]/{topicId:Guid}")]
        public async Task<IActionResult> ChatMessages(Guid topicId)
        {
            var pushMessages = new List<PushMessage>();
            var subscriptions = await _topicService.GetSubscriptionsByTopicId(topicId);
            if (!subscriptions.Any()) return Ok(pushMessages);

            var messages = await _topicService.GetMessagesBySubscriptionIds(subscriptions.Select(s => s.Id).ToList());
            if (!messages.Any()) return Ok(pushMessages);

            var result = from m in messages
                         join s in subscriptions on m.CreatedBy equals s.Email
                         select new PushMessage { MessageId = m.Id, TopicId = m.TopicId, Type = SubscriptionType.Chat, Title = m.Title, Content = m.Content, CreatedBy = s.Email, CreatedOn = m.CreatedOn };
            pushMessages = result.OrderBy(m => m.CreatedOn).ToList();
            return Ok(pushMessages);
        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Notifications()
        {
            var subscriptions = await _topicService.GetUserSubscriptionsByType(TopicToSubscriptionMapping[TopicType.Notify]);
            var messages = await _topicService.GetMessagesBySubscriptionIds(subscriptions.Select(s => s.Id).ToList());
            var notificationMessages = messages.Select(m => new PushMessage { MessageId = m.Id, TopicId = m.TopicId, Type = SubscriptionType.Push, Title = m.Title, Content = m.Content, CreatedOn = m.CreatedOn });
            return Ok(notificationMessages.OrderByDescending(m => m.CreatedOn));
        }
    }
}