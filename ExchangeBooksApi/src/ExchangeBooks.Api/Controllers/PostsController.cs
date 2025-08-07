using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExchangeBooks.Infra.Interfaces;
using ExchangeBooks.Infra.Models.Domain;
using ExchangeBooks.Infra.Models.Request;
using System.Linq;
using System.Collections.Generic;
using ExchangeBooks.Infra;
using FirebaseAdmin.Messaging;
using ExchangeBooks.Infra.Enums;
using static ExchangeBooks.Infra.Constants;
using FcmMessage = FirebaseAdmin.Messaging.Message;
using Message = ExchangeBooks.Infra.Models.Domain.Message;

namespace ExchangeBooks.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostsService _postsService;
        private readonly IMapper _mapper;
        private readonly IMessagesService _topicService;
        private readonly IEmailService _emailService;
        public PostsController(IPostsService postsService, IMapper mapper
        , IMessagesService topicService, IEmailService emailService)
        {
            _postsService = postsService;
            _mapper = mapper;
            _topicService = topicService;
            _emailService = emailService;
        }

        [Authorize("checkwritescope")]
        [HttpPost("[action]")]
        public async Task<IActionResult> Add([FromBody] PostRequest postRequest)
        {
            Message storeMessage;
            var post = _mapper.Map<PostRequest, Post>(postRequest);
            var postResponse = await _postsService.AddPost(post);
            if (postResponse == null)
                return StatusCode(StatusCodes.Status500InternalServerError);
            await postResponse.Books.ToList().ForEachAsync(async b =>
            {
                var topics = await _topicService.GetTopicsByTag(string.Join("|", b.Tags));
                await topics.ForEachAsync(async t =>
                {
                    if (t.Type != TopicType.Notify) return;
                    var topicSubscriptions = await _topicService.GetActiveSubscriptionsByTopicId(t.Id);
                    if (!topicSubscriptions.Any()) return;
                    await _topicService.AddPostNamesToTopic(t.Id, new List<string> { postResponse.Name });
                    storeMessage = await _topicService.AddMessage(t.Id, topicSubscriptions.Select(ts => ts.Id).ToList(), postResponse.Name, $"A new book {b.Name} added");
                    var message = new FcmMessage
                    {
                        Data = new Dictionary<string, string>
                        {
                            { Constants.PushMessages.MessageId, $"{storeMessage.Id}" },
                            { Constants.PushMessages.TopicId, $"{storeMessage.TopicId}" },
                            { Constants.PushMessages.Type, $"{TopicToSubscriptionMapping[t.Type]}" },
                            { Constants.PushMessages.Title, "New book" },
                            { Constants.PushMessages.Content, storeMessage.Content },
                            { Constants.PushMessages.CreatedBy, storeMessage.CreatedBy },
                            { Constants.PushMessages.CreatedOn, $"{storeMessage.CreatedOn}" }
                        },
                        Topic = t.FcmId,
                        Apns = new ApnsConfig
                        {
                            Aps = new Aps
                            {
                                Alert = new ApsAlert
                                {
                                    Title = storeMessage.Title,
                                    Body = storeMessage.Content
                                }
                            }
                        }
                    };
                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                });
            });
            return Ok(postResponse);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var post = await _postsService.GetPost(id);
            var postResponse = _mapper.Map<Post, PostRequest>(post);
            return Ok(postResponse);
        }

        [HttpGet("[action]/{count}")]
        public async Task<IActionResult> RecentBooks(int count)
        {
            var books = await _postsService.GetRecentBooks(count);
            return Ok(books);
        }

        [HttpGet("[action]/{tags}")]
        public async Task<IActionResult> Search(string tags)
        {
            var query = tags.Split(Constants.TagSeparator).Select(t => t.ToLower()).ToList();
            var result = await _postsService.SearchBooks(query);
            return Ok(result);
        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]")]
        public async Task<IActionResult> UserPosts()
        {
            var response = await _postsService.GetUserPosts();
            return Ok(response);
        }

        [Authorize("checkwritescope")]
        [HttpPatch("[action]/{id}/{postStatus}")]
        public async Task<IActionResult> Status(Guid id, string postStatus)
        {
            if (!Enum.TryParse(typeof(PostStatus), postStatus, true, out var status))
                return BadRequest();
            await _postsService.MarkPostStatus(id, (PostStatus)status);
            return Ok();
        }

        [Authorize("checkwritescope")]
        [HttpPatch("{postId}/[action]/{id}/{bookStatus}")]
        public async Task<IActionResult> BookStatus(Guid postId, Guid id, string bookStatus)
        {
            if (!Enum.TryParse(typeof(BookStatus), bookStatus, true, out var status))
                return BadRequest();
            await _postsService.MarkBookStatus(postId, id, (BookStatus)status);
            return Ok();
        }

        [Authorize("checkwritescope")]
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _postsService.DeletePost(id);
            return NoContent();
        }

        [Authorize("checkwritescope")]
        [HttpDelete("{postId}/[action]/{id}")]
        public async Task<IActionResult> DeleteBook(Guid postId, Guid id)
        {
            await _postsService.DeleteBook(postId, id);
            return NoContent();
        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]")]
        public async Task<IActionResult> HidePost()
        {
            var hidePosts = await _postsService.GetHidePosts();
            if (hidePosts is null)
                return NotFound();
            return Ok(hidePosts);
        }

        [Authorize("checkwritescope")]
        [HttpPost("[action]")]
        public async Task<IActionResult> HidePost([FromBody] HidePostRequest request)
        {
            if(!request.PostId.HasValue && string.IsNullOrEmpty(request.UserEmail))
                return BadRequest();
            await _postsService.AddHidePosts(request);
            return Ok();
        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]")]
        public async Task<IActionResult> FlagPost()
        {
            var flagPosts = await _postsService.GetFlagPosts();
            if (flagPosts is null)
                return NotFound();
            return Ok(flagPosts);
        }

        [Authorize("checkwritescope")]
        [HttpPost("[action]")]
        public async Task<IActionResult> FlagPost([FromBody] FlagPostRequest request)
        {
            var body = $"Check postId {request.PostId.ToString()} for reason {nameof(request.Reason)}";
            await _emailService.SendEmail("sudherson.v@gmail.com", "Sudherson V", body, "Post flagged");
            await _postsService.AddFlagPost(request);
            return Ok();
        }
    }
}