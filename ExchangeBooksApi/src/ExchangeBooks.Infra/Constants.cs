using System.Collections.Generic;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Models.Domain;

namespace ExchangeBooks.Infra
{
    public class Constants
    {
        public class IdServer
        {
            // public const string Authority = "https://localhost:6001/";
            public const string Authority = "https://exchangebooksidentityserver.azurewebsites.net";

        }
        public class ClaimTypes
        {
            public const string AccessToken = "accesstoken";
        }
        public static List<string> SearchParams = new List<string> {
            nameof(Book.Condition), nameof(Book.Class), nameof(Book.Price)
        };

        public const char TagSeparator = '|';
        public const char TagValueSeparator = '=';
        public const int RandomStringSize = 50;

        public static IDictionary<TopicType, SubscriptionType> TopicToSubscriptionMapping = new Dictionary<TopicType, SubscriptionType>
        {
            { TopicType.Notify, SubscriptionType.Push },
            { TopicType.Chat, SubscriptionType.Chat }
        };

        public class PushMessages
        {
            public const string MessageId = "messageId";
            public const string TopicId = "topicId";
            public const string Type = "type";
            public const string Title = "title";
            public const string Content = "content";
            public const string CreatedBy = "createdBy";
            public const string CreatedOn = "createdOn";
        }
    }
}
