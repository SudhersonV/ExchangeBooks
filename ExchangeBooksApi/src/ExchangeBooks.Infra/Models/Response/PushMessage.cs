using System;
using ExchangeBooks.Infra.Enums;

namespace ExchangeBooks.Infra.Models.Response
{
    public class PushMessage
    {
        public Guid MessageId { get; set; }
        public Guid TopicId { get; set; }
        public SubscriptionType Type { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}