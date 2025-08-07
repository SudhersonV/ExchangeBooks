using System;
using ExchangeBooks.Enums;

namespace ExchangeBooks.Models
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
