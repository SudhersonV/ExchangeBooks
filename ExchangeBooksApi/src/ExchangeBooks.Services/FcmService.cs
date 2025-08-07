using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExchangeBooks.Infra;
using ExchangeBooks.Infra.Interfaces;
using ExchangeBooks.Infra.Models.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace ExchangeBooks.Services
{
    public class FcmService : IFcmService
    {
        private Container _fcmContainer;
        private readonly IClaimsAccessor _claimsAccessor;
        private readonly CosmosDbOptions _cosmosDbOptions;

        public FcmService(CosmosClient dbClient, IClaimsAccessor claimsAccessor, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _claimsAccessor = claimsAccessor;
            _cosmosDbOptions = cosmosDbOptions.Value;
            _fcmContainer = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.FcmContainerId);
        }

        public string GenerateFcmTopicId(string topicName, Guid topicId)
        {
            //Only allow "[a-zA-Z0-9-_.~%]+"
            var strippedTopicName = Regex.Replace(topicName, @"[^a-zA-Z0-9-_.~%]", "");
            return $"{strippedTopicName}~{topicId}";
        }

        public async Task<string> GetToken(string email = null)
        {
            var fcm = await GetUserFcm(email);
            return fcm?.Token;
        }

        public async Task SetToken(string token)
        {
            var email = _claimsAccessor.Email;
            try
            {
                var fcm = await GetUserFcm(email);
                if (fcm != null) return;
                fcm = new Fcm
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Token = token,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };
                await _fcmContainer.CreateItemAsync(fcm);
            }
            catch (Exception ex)
            {
                var x = ex;
            }
        }

        public async Task UpdateToken(string token)
        {
            var email = _claimsAccessor.Email;
            try
            {
                var fcm = await GetUserFcm(email);
                if (fcm is null) return;

                fcm.Token = token;
                fcm.ModifiedOn = DateTime.UtcNow;
                await _fcmContainer.ReplaceItemAsync<Fcm>(fcm, fcm.Id.ToString());
            }
            catch (Exception ex)
            {
                var x = ex;
            }
        }

        public async Task Clean()
        {
            var iterator = _fcmContainer.GetItemLinqQueryable<Fcm>(true).ToFeedIterator();
            var searchResults = await iterator.ReadNextAsync();
            await searchResults.ToList().ForEachAsync(async (p) =>
            {
                await _fcmContainer.DeleteItemAsync<Fcm>(p.Id.ToString(), new PartitionKey(p.Email.ToString()));
            });
        }

        private async Task<Fcm> GetUserFcm(string email = null)
        {
            if (string.IsNullOrEmpty(email))
                email = _claimsAccessor.Email;
            try
            {
                var iterator = _fcmContainer.GetItemLinqQueryable<Fcm>(true)
                    .Where(f => f.Email == email)
                    .Take(1).ToFeedIterator();
                var fcms = await iterator.ReadNextAsync();
                return fcms.FirstOrDefault();
            }
            catch (Exception ex)
            {
                var x = ex;
                return null;
            }
        }

        private string GetRandomString()
        {
            var _random = new Random();
            var builder = new StringBuilder(Constants.RandomStringSize);
            // char is a single Unicode character  
            char offset = 'a';
            const int lettersOffset = 26; // A...Z or a..z: length = 26  
            for (var i = 0; i < Constants.RandomStringSize; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }
            return builder.ToString().ToLower();
        }
    }
}