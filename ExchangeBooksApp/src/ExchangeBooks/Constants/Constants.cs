
namespace ExchangeBooks.Constants
{
    public class Constants
    {
        public const string EnvPrefix = "qa-";
        //public const string EnvPrefix = "";

        public static class IdentityServer
        {

            public static string Url = $"https://{EnvPrefix}exchangebooksidentityserver.azurewebsites.net";

            public class Paths
            {
                public const string Login = "account/logout";
                public const string Eula = "home/eula";
                public const string Support = "home/support";
            }
        }

        public static class Auth
        {
            public static string Url = $"https://{EnvPrefix}exchangebooksauthapi.azurewebsites.net";

            public const string RedirectUrl = "xamarinessentials://";

            public class Paths
            {
                public const string Login = "mobileauth/oidc";
                public const string Refresh = "mobileauth/refresh";
                public const string Eula = "account/eula";
            }
        }

        public static class Api
        {
            public static string Url = $"https://{EnvPrefix}exchangebooksapi.azurewebsites.net";

            public class Paths
            {
                public class Post
                {
                    public const string AddPost = "posts/add";
                    public const string RecentBooks = "posts/recentBooks/{count}";
                    public const string SearchBooks = "posts/search/{tags}";
                    public const string UserPosts = "posts/userposts";
                    public const string GetPostById = "posts/get/{id}";
                    public const string MarkPostStatus = "posts/status/{id}/{postStatus}";
                    public const string MarkBookStatus = "posts/{postId}/bookStatus/{id}/{bookStatus}";
                    public const string DeletePost = "posts/delete/{id}";
                    public const string DeleteBook = "posts/{postId}/deleteBook/{id}";
                    public const string HiddenPosts = "posts/hidePost";
                    public const string HidePost = "posts/hidePost";
                    public const string FlaggedPosts = "posts/flagPost";
                    public const string FlagPost = "posts/flagPost";
                }
                public class Message
                {
                    public const string SubscribeToTopic = "messages/subscribeToTopic/{topicType}/{topicName}";
                    public const string UnsubscribeToTopic = "messages/unsubscribeTopic/{id}";
                    public const string UserTopics = "messages/topics/{topicType}";
                    public const string ChatMessages = "messages/chatMessages/{topicId}";
                    public const string Notifications = "messages/notifications";
                    public const string SendMessage = "messages/message/{topicId}";
                    public const string GetTopic = "messages/topic/{id}";
                }

                public class Fcm
                {
                    public const string SetToken = "fcm/token";
                }
            }
        }
        public const string TagSeparator = "|";
        public static readonly char[] SearchSeparator = new char[] { ' ', ',', ':', ';', '-', '&' };
        public const string NoContent = "No content available!";

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

        public class Messaging
        {
            public const string PushMessage = "pushMessage";
            public const string ChatMessage = "chatMessage";
            public const string PendingMessage = "pendingMessage";
            public const int ScrollThreshold = 6;
        }
    }
}
