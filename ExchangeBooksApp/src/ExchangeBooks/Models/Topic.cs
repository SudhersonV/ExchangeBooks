using System;
using System.Collections.Generic;
using ExchangeBooks.Enums;

namespace ExchangeBooks.Models
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> PostNames { get; set; } = new List<string>();
        public string FcmId { get; set; }
        public TopicType Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int PendingMessageCount { get; set; }
        public bool HasPendingMessage => PendingMessageCount > 0;
    }
}
