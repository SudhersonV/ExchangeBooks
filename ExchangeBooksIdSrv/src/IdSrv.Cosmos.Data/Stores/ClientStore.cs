using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdSrv.Infra;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.ConfigurationStore
{
    public class ClientStore : IClientStore
    {
        private Container _container;
        private readonly CosmosDbOptions _cosmosDbOptions;

        public ClientStore(CosmosClient dbClient, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _cosmosDbOptions = cosmosDbOptions.Value;
            _container = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.ClientsContainerId);
        }

        public async Task AddItemAsync(Client client)
        {
            await _container.CreateItemAsync<Client>(client, new PartitionKey(client.ClientId));
        }

        public async Task DeleteItemAsync(string clientId)
        {
            await _container.DeleteItemAsync<Client>(clientId, new PartitionKey(clientId));
        }
       
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            try
            {
                var iterator = _container.GetItemLinqQueryable<Client>(true)
                    .Where(c => c.ClientId == clientId).ToFeedIterator();
                var clients = await iterator.ReadNextAsync();
                return clients.FirstOrDefault();
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
        }

        public async Task<IEnumerable<Client>> GetItemsAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<Client>(new QueryDefinition(queryString));
            List<Client> results = new List<Client>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, Client item)
        {
            await _container.UpsertItemAsync<Client>(item, new PartitionKey(id));
        }        
    }
}
