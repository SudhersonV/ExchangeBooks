using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdSrv.Cosmos.Data.Entities;
using IdSrv.Infra;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.ConfigurationStore
{
    public class PersistedGrantStore : IPersistedGrantStore
    {
        private Container _container;
        private readonly CosmosDbOptions _cosmosDbOptions;
        private readonly IMapper _mapper;

        public PersistedGrantStore(CosmosClient dbClient, IMapper mapper, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _mapper = mapper;
            _cosmosDbOptions = cosmosDbOptions.Value;
            _container = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.PersistedGrantsContainerId);
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var grants = new List<PersistedGrant>().AsEnumerable();
            try
            {
                var iterator = _container.GetItemLinqQueryable<PersistedGrantEntity>(true)
                    .Where(pg => pg.SubjectId == subjectId).ToFeedIterator();
                var grantEntities = await iterator.ReadNextAsync();
                grants = _mapper.Map<List<PersistedGrantEntity>, List<PersistedGrant>>(grantEntities.ToList());
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
            return grants;
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            try
            {
                key = EncodePersistedGrantKey(key);
                var iterator = _container.GetItemLinqQueryable<PersistedGrantEntity>(true)
                    .Where(c => c.Key == key).ToFeedIterator();
                var grants = await iterator.ReadNextAsync();
                var grant = _mapper.Map<PersistedGrantEntity, PersistedGrant>(grants.First());
                return grant;
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
            try
            {
                grant.Key = EncodePersistedGrantKey(grant.Key);
                var grantEntity = _mapper.Map<PersistedGrant, PersistedGrantEntity>(grant);
                await _container.CreateItemAsync(grantEntity);
            }
            catch (CosmosException ex)
            {
                var x = ex;
            }
        }

        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            try
            {
                var iterator = _container.GetItemLinqQueryable<PersistedGrantEntity>(true)
                    .Where(pg => pg.SubjectId == subjectId && pg.ClientId == clientId).ToFeedIterator();
                var grants = await iterator.ReadNextAsync();
                await grants.ToList().ForEachAsync(async g =>
                {
                    await _container.DeleteItemAsync<PersistedGrantEntity>(g.Key, new PartitionKey(g.Key));
                });
            }
            catch (CosmosException ex)
            {
                var x = ex;
            }
        }

        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            try
            {
                var iterator = _container.GetItemLinqQueryable<PersistedGrantEntity>(true)
                    .Where(pg => pg.SubjectId == subjectId && pg.ClientId == clientId && pg.Type == type).ToFeedIterator();
                var grants = await iterator.ReadNextAsync();
                await grants.ToList().ForEachAsync(async g =>
                {
                    await _container.DeleteItemAsync<PersistedGrantEntity>(g.Key, new PartitionKey(g.Key));
                });
            }
            catch (CosmosException ex)
            {
                var x = ex;
            }
        }

        public async Task RemoveAsync(string key)
        {
            key = EncodePersistedGrantKey(key);
            await _container.DeleteItemAsync<PersistedGrantEntity>(key, new PartitionKey(key));
        }

        private static string EncodePersistedGrantKey(string key)
        {
            key = Base64UrlEncoder.Encode(key);
            return key;
        }
    }
}
