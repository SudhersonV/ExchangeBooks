using System;
using System.Collections.Generic;
using ExchangeBooks.Infra.Enums;

namespace ExchangeBooks.Infra.Models.Domain
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> PostNames{ get; set; } = new List<string>();
        public string FcmId { get; set; }
        public TopicType Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class Subscription
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid TopicId { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public SubscriptionType Type { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }

    public class Message
    {
        public Guid Id { get; set; }
        public Guid TopicId { get; set; }
        public List<Guid> SubscriptionIds { get; set; } = new List<Guid>();
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}