using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdSrv.Infra;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.ConfigurationStore
{
    public class ResourceStore : IResourceStore
    {
        private Container _apiResourcesContainer, _identityResourcesContainer;
        private readonly CosmosDbOptions _cosmosDbOptions;

        public ResourceStore(CosmosClient dbClient, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _cosmosDbOptions = cosmosDbOptions.Value;
            _apiResourcesContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.ApiResourcesContainerId);
            _identityResourcesContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.IdentityResourcesContainerId);
        }

        public async Task<ApiResource> FindApiResourceAsync(string name)
        {
            try
            {
                var iterator = _apiResourcesContainer.GetItemLinqQueryable<ApiResource>(true)
                    .Where(c => c.Name == name).ToFeedIterator();
                var resources = await iterator.ReadNextAsync();
                return resources.First();
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));
            var apiResources = new List<ApiResource>().AsEnumerable();
            try
            {
                var iterator = _apiResourcesContainer.GetItemLinqQueryable<ApiResource>(true)
                .Where(ir => ir.Scopes.Any(s => scopeNames.Contains(s.Name))).ToFeedIterator();
                apiResources = await iterator.ReadNextAsync();
            }
            catch (CosmosException ex)
            {
                var x = ex;
            }
            return apiResources;
        }

        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));
            var identityResources = new List<IdentityResource>().AsEnumerable();
            try
            {
                var iterator = _identityResourcesContainer.GetItemLinqQueryable<IdentityResource>(true)
                    .Where(ir => scopeNames.Contains(ir.Name)).ToFeedIterator();
                identityResources = await iterator.ReadNextAsync();
            }
            catch (CosmosException ex)
            {
                var x = ex;
            }
            return identityResources;
        }

        public async Task<Resources> GetAllResourcesAsync()
        {
            try
            {
                var apiResourcesIterator = _apiResourcesContainer.GetItemLinqQueryable<ApiResource>(true).ToFeedIterator();
                var apiResources = await apiResourcesIterator.ReadNextAsync();
                var identityResourcesIterator = _identityResourcesContainer.GetItemLinqQueryable<IdentityResource>(true).ToFeedIterator();
                var identityResources = await identityResourcesIterator.ReadNextAsync();
                var result = new Resources(identityResources, apiResources);
                return result;
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
        }
    }
}
