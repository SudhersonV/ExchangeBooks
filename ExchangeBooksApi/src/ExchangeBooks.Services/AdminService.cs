using System.Threading.Tasks;
using ExchangeBooks.Infra.Interfaces;
using Microsoft.Azure.Cosmos.Scripts;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ExchangeBooks.Infra.Models.Domain;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq;
using ExchangeBooks.Infra;

namespace ExchangeBooks.Services
{
    public class AdminService : IAdminService
    {
        private CosmosClient _dbClient;
        private Container _fcmContainer, _postsContainer, _topicContainer, _subscriptionContainer
        , _messageContainer, _hidePostsContainer, _flagPostsContainer;
        private readonly CosmosDbOptions _cosmosDbOptions;

        public AdminService(CosmosClient dbClient, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _cosmosDbOptions = cosmosDbOptions.Value;
            _dbClient = dbClient;
            _fcmContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.FcmContainerId);
            _topicContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.TopicsContainerId);
            _subscriptionContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.SubscriptionsContainerId);
            _messageContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.MessagesContainerId);
            _postsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.PostsContainerId);
            _hidePostsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.HidePostsContainerId);
            _flagPostsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.FlagPostsContainerId);
        }
        public async Task Clean()
        {
            //Fcm
            var fcmIterator = _fcmContainer.GetItemLinqQueryable<Fcm>(true).ToFeedIterator();
            try
            {
                var fcmSearchResults = await fcmIterator.ReadNextAsync();
                await fcmSearchResults.ToList().ForEachAsync(async (p) =>
                {
                    await _fcmContainer.DeleteItemAsync<Fcm>(p.Id.ToString(), new PartitionKey(p.Email.ToString()));
                });
            }
            catch { }

            //Posts
            var postIterator = _postsContainer.GetItemLinqQueryable<Post>(true).ToFeedIterator();
            try
            {
                var postSearchResults = await postIterator.ReadNextAsync();
                await postSearchResults.ToList().ForEachAsync(async (p) =>
                {
                    await _postsContainer.DeleteItemAsync<Post>(p.Id.ToString(), new PartitionKey(p.Id.ToString()));
                });
            }
            catch { }

            //Topics
            var topicIterator = _topicContainer.GetItemLinqQueryable<Topic>(false).ToFeedIterator();
            try
            {
                var topicSearchResults = await topicIterator.ReadNextAsync();
                await topicSearchResults.ToList().ForEachAsync(async (p) =>
                {
                    await _topicContainer.DeleteItemAsync<Topic>(p.Id.ToString(), new PartitionKey(p.Name.ToString()));
                });
            }
            catch { }

            //Subscriptions
            var subIterator = _subscriptionContainer.GetItemLinqQueryable<Subscription>(true).ToFeedIterator();
            try
            {
                var subSearchResults = await subIterator.ReadNextAsync();
                await subSearchResults.ToList().ForEachAsync(async (p) =>
                {
                    await _subscriptionContainer.DeleteItemAsync<Subscription>(p.Id.ToString(), new PartitionKey(p.Name.ToString()));
                });
            }
            catch { }

            //Messages
            var msgIterator = _messageContainer.GetItemLinqQueryable<Message>(true).ToFeedIterator();
            try
            {
                var msgSearchResults = await msgIterator.ReadNextAsync();
                await msgSearchResults.ToList().ForEachAsync(async (p) =>
                {
                    await _messageContainer.DeleteItemAsync<Message>(p.Id.ToString(), new PartitionKey(p.Id.ToString()));
                });
            }
            catch { }

            //HidePosts
            var hidePostsIterator = _hidePostsContainer.GetItemLinqQueryable<HidePosts>(true).ToFeedIterator();
            try
            {
                var hidePostsResults = await hidePostsIterator.ReadNextAsync();
                await hidePostsResults.ToList().ForEachAsync(async (p) =>
                {
                    await _messageContainer.DeleteItemAsync<HidePosts>(p.Id.ToString(), new PartitionKey(p.Email));
                });
            }
            catch { }

            //FlagPosts
            var flagPostsIterator = _flagPostsContainer.GetItemLinqQueryable<FlagPosts>(true).ToFeedIterator();
            try
            {
                var flagPostsResults = await flagPostsIterator.ReadNextAsync();
                await flagPostsResults.ToList().ForEachAsync(async (p) =>
                {
                    await _messageContainer.DeleteItemAsync<FlagPosts>(p.Id.ToString(), new PartitionKey(p.Email.ToString()));
                });
            }
            catch { }
        }

        public async Task Delete()
        {
            try
            {
                await _fcmContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _postsContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _topicContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _subscriptionContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _messageContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _hidePostsContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _flagPostsContainer.DeleteContainerAsync();
            }
            catch { }
        }

        public async Task Create()
        {
            await _dbClient.CreateDatabaseIfNotExistsAsync(_cosmosDbOptions.DatabaseId);
            
            var database = _dbClient.GetDatabase(_cosmosDbOptions.DatabaseId);

            //Fcm
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.FcmContainerId, "/email");
            await CreateStorProcs(_fcmContainer, "Fcm");
            //Posts
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.PostsContainerId, "/id");
            await CreateStorProcs(_postsContainer, "Posts");
            //Topics
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.TopicsContainerId, "/name");
            await CreateStorProcs(_topicContainer, "Topics");
            //Subscriptions
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.SubscriptionsContainerId, "/name");
            await CreateStorProcs(_subscriptionContainer, "Subscriptions");
            //Messages
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.MessagesContainerId, "/id");
            await CreateStorProcs(_messageContainer, "Messages");
            //HidePosts
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.HidePostsContainerId, "/email");
            await CreateStorProcs(_hidePostsContainer, "HidePosts");
            //FlagPosts
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.FlagPostsContainerId, "/email");
            await CreateStorProcs(_flagPostsContainer, "FlagPosts");
        }

        private async Task CreateStorProcs(Container container, string containerId)
        {
            var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/StoredProcedures/{containerId}";
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists) return;
            var storProcs = directoryInfo.GetFiles().ToList();
            await storProcs.ForEachAsync(async sp =>
            {
                var spId = sp.Name.Replace(@".js", "");
                try
                {
                    await container.Scripts.DeleteStoredProcedureAsync(spId);
                }
                catch { }
                await container.Scripts.CreateStoredProcedureAsync(new StoredProcedureProperties
                {
                    Id = spId,
                    Body = File.ReadAllText($"{sp.Directory.FullName}/{sp.Name}")
                });
            });
        }
    }
}