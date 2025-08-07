using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeBooks.Infra;
using ExchangeBooks.Infra.Interfaces;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExchangeBooks.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FcmController : ControllerBase
    {
        private readonly ILogger<FcmController> _logger;
        private readonly IFcmService _fcmService;
        private readonly IMessagesService _topicService;

        public FcmController(ILogger<FcmController> logger, IFcmService fcmService,
        IMessagesService topicService)
        {
            _logger = logger;
            _fcmService = fcmService;
            _topicService = topicService;

        }

        [Authorize("checkwritescope")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Token()
        {
            var token = await _fcmService.GetToken();
            if (token is null)
                return NotFound();
            else
                return Ok(token);
        }

        [Authorize("checkwritescope")]
        [HttpPost("[action]")]
        public async Task<IActionResult> Token([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid token");

            var existingToken = await _fcmService.GetToken();
            if (existingToken is null)
                await _fcmService.SetToken(token);
            else
            {
                await _fcmService.UpdateToken(token);
                var subscriptions = await _topicService.GetUserSubscriptions();
                if (!subscriptions.Any())
                    return Ok(token);

                await subscriptions.Select(s => s.TopicId).Distinct().ToList().ForEachAsync(async topicId =>
                {
                    var topic = await _topicService.GetTopicById(topicId);
                    if (topic == null) return;
                    await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(new List<string> { existingToken }, topic.FcmId);
                    await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(new List<string> { token }, topic.FcmId);
                });
            }
            return Ok(token);
        }
    }
}