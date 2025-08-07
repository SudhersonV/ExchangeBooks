using System;
namespace ExchangeBooks.Models
{
    public class PendingMessage
    {
        public Guid TopicId { get; set; }
        public int Count { get; set; }
    }
}
