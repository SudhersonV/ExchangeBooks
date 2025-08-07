using IdentityServer4.Models;
using IdentityServer4.Services;
using IdSrv.Infra;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.ConfigurationStore
{
    public class CorsPolicyService : ICorsPolicyService
    {
        private Container _container;
        private readonly CosmosDbOptions _cosmosDbOptions;
        public CorsPolicyService(CosmosClient dbClient, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _cosmosDbOptions = cosmosDbOptions.Value;
            _container = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.ClientsContainerId);
        }
        public async Task<bool> IsOriginAllowedAsync(string origin)
        {            
            var iterator = _container.GetItemLinqQueryable<Client>(true).Where(c => c != null)
                .SelectMany(c => c.AllowedCorsOrigins).ToFeedIterator();
            var origins = await iterator.ReadNextAsync();
            var distinctOrigins = origins.Distinct();
            var isAllowed = distinctOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
            return isAllowed;
        }
    }
}
