using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdSrv.Cosmos.Data.Interfaces;
using IdSrv.Infra;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serenity;
using IdSrv.Infra.Models;
using IdSrv.Cosmos.Data.Entities;
using Microsoft.Azure.Cosmos.Linq;

namespace IdSrv.Cosmos.Data.Services
{
    public class AdminService : IAdminService
    {
        private CosmosClient _dbClient;
        private Container _apiRscContainer, _appUsrContainer, _clientsContainer, _idRscContainer, _persistedGrantsContainer;
        private readonly CosmosDbOptions _cosmosDbOptions;
        public AdminService(CosmosClient dbClient, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _cosmosDbOptions = cosmosDbOptions.Value;
            _dbClient = dbClient;
            _apiRscContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.ApiResourcesContainerId);
            _appUsrContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.ApplicationUsersContainerId);
            _clientsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.ClientsContainerId);
            _idRscContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.IdentityResourcesContainerId);
            _persistedGrantsContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.PersistedGrantsContainerId);
        }

        public async Task Create()
        {
            await _dbClient.CreateDatabaseIfNotExistsAsync(_cosmosDbOptions.DatabaseId);
            
            var database = _dbClient.GetDatabase(_cosmosDbOptions.DatabaseId);

            //ApiResources
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.ApiResourcesContainerId, "/name");
            await SeedData<ApiResourceC>(_apiRscContainer, "ApiResources", "name");
            //ApplicationUsers
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.ApplicationUsersContainerId, "/id");
            //Clients
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.ClientsContainerId, "/clientId");
            await SeedData<ClientC>(_clientsContainer, "Clients", "clientId");
            //IdentityResources
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.IdentityResourcesContainerId, "/name");
            await SeedData<IdentityResourceC>(_idRscContainer, "IdentityResources", "name");
            //PersistedGrants
            await database.CreateContainerIfNotExistsAsync(_cosmosDbOptions.PersistedGrantsContainerId, "/id");
        }

        private async Task SeedData<T>(Container container, string containerId, string partitionKey)
        {
            var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Seed/{containerId}";
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists) return;
            var jsonFileInfos = directoryInfo.GetFiles().ToList();
            await jsonFileInfos.ForEachAsync(async fileInfo =>
            {
                var file = File.ReadAllText($"{fileInfo.Directory.FullName}/{fileInfo.Name}");
                var jObject = JObject.Parse(file);
                var body = JSON.Parse<T>(file);
                try
                {
                    var id = jObject["id"];
                    var pkey = jObject[partitionKey];
                    await container.DeleteItemAsync<T>(id.ToString(), new PartitionKey(pkey.ToString()));
                }
                catch { }
                await container.CreateItemAsync(body);
            });
        }

        public async Task Delete()
        {
            try
            {
                await _apiRscContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _appUsrContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _clientsContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _idRscContainer.DeleteContainerAsync();
            }
            catch { }
            try
            {
                await _persistedGrantsContainer.DeleteContainerAsync();
            }
            catch { }
        }

        public async Task Clean()
        {
            //Application Users
            var appUsersIterator = _appUsrContainer.GetItemLinqQueryable<ApplicationUser>(true).ToFeedIterator();
            try
            {
                var appUsersSearchResults = await appUsersIterator.ReadNextAsync();
                await appUsersSearchResults.ToList().ForEachAsync(async (p) =>
                {
                    await _appUsrContainer.DeleteItemAsync<ApplicationUser>(p.Id.ToString(), new PartitionKey(p.Id.ToString()));
                });
            }
            catch { }

            //Persisted Grants
            var grantsIterator = _persistedGrantsContainer.GetItemLinqQueryable<PersistedGrantEntity>(true).ToFeedIterator();
            try
            {
                var grantsSearchResults = await grantsIterator.ReadNextAsync();
                await grantsSearchResults.ToList().ForEachAsync(async (p) =>
                {
                    await _persistedGrantsContainer.DeleteItemAsync<PersistedGrantEntity>(p.Key.ToString(), new PartitionKey(p.Key.ToString()));
                });
            }
            catch { }
        }
    }
}