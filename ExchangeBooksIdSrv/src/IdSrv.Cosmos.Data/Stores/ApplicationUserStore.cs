using AutoMapper;
using IdSrv.Cosmos.Data.Entities;
using IdSrv.Cosmos.Data.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Security.Claims;
using IdentityModel;
using System.IdentityModel.Tokens.Jwt;
using IdSrv.Infra;
using Microsoft.Extensions.Options;

namespace IdSrv.Cosmos.Data.Stores
{
    public class ApplicationUserStore : IApplicationUserStore<ApplicationUser>
    {
        private Container _container;
        private readonly IMapper _mapper;
        private readonly CosmosDbOptions _cosmosDbOptions;

        public ApplicationUserStore(CosmosClient dbClient, IMapper mapper
        , IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _mapper = mapper;
            _cosmosDbOptions = cosmosDbOptions.Value;
            _container = dbClient.GetContainer(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.ApplicationUsersContainerId);
        }
        /// <summary>
        /// Finds the user by external provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public async Task<ApplicationUser> FindByExternalProvider(string provider, string userId)
        {
            var iterator = _container.GetItemLinqQueryable<ApplicationUser>(true).ToFeedIterator();
            var users = await iterator.ReadNextAsync();
            return users.FirstOrDefault(u => u.ProviderName == provider && u.ProviderSubjectId == userId);
        }

        /// <summary>
        /// Automatically provisions a user.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public async Task<ApplicationUser> AutoProvisionUser(string provider, string userId, List<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            foreach (var claim in claims)
            {
                // if the external system sends a display name - translate that to the standard OIDC name claim
                if (claim.Type == ClaimTypes.Name)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, claim.Value));
                }
                // if the JWT handler has an outbound mapping to an OIDC claim use that
                else if (JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.ContainsKey(claim.Type))
                {
                    filtered.Add(new Claim(JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap[claim.Type], claim.Value));
                }
                // copy the claim as-is
                else
                {
                    filtered.Add(claim);
                }
            }

            // if no display name was provided, try to construct by first and/or last name
            if (!filtered.Any(x => x.Type == JwtClaimTypes.Name))
            {
                var first = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value;
                var last = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value;
                if (first != null && last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            // create a new unique subject id
            var sub = CryptoRandom.CreateUniqueId(format: CryptoRandom.OutputFormat.Hex);

            // check if a display name is available, otherwise fallback to subject id
            var name = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value ?? sub;
            var email = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value ?? sub;
            var firstName = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value ?? sub;
            var lastName = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.FamilyName)?.Value ?? sub;

            // create new user
            var user = new ApplicationUser
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ProviderName = provider,
                ProviderSubjectId = userId
            };

            // add user to in-memory store
            var userCreate = await CreateUserAsync(user);
            return userCreate.Item2;
        }

        public async Task<(HttpStatusCode, ApplicationUser)> CreateUserAsync(ApplicationUser user)
        {
            try
            {
                var existingUser = await FindByEmailAsync(user.Email, user.ProviderName);
                //Only for local user creation
                if (existingUser != null && user.ProviderName == Constants.ExchangeBooks)
                {
                    if (!existingUser.EmailConfirmed)
                        await DeleteUserAsync(existingUser.Id);
                    else
                        return (HttpStatusCode.Conflict, null);
                }
                user.Id = Guid.NewGuid();
                user.UserName = user.Email;
                user.NormalizedUserName = user.Email;
                user.NormalizedEmail = user.Email;
                if (user.ProviderName == Constants.ExchangeBooks)
                    user.ProviderSubjectId = user.Email;
                await _container.CreateItemAsync(user);
                return (HttpStatusCode.OK, user);

            }
            catch (CosmosException ex)
            {
                var x = ex;
                return (ex.StatusCode, null);
            }
        }

        public async Task<string> GetSecurityStampAsync(ApplicationUser user)
        {
            try
            {
                return await Task.FromResult(user.SecurityStamp);
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
        }

        public async Task SetSecurityStampAsync(ApplicationUser user, string stamp)
        {
            try
            {
                user.SecurityStamp = stamp;
                await _container.ReplaceItemAsync(user, user.Id.ToString());
            }
            catch (CosmosException ex)
            {
                var x = ex;
            }
        }

        public async Task<Guid> GetUserIdAsync(ApplicationUser user)
        {
            try
            {
                return await Task.FromResult(user.Id);
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return Guid.Empty;
            }
        }

        public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
        {
            try
            {
                return await Task.FromResult(user.EmailConfirmed);
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return false;
            }
        }

        public async Task<string> GetEmailAsync(ApplicationUser user)
        {
            try
            {
                return await Task.FromResult(user.Email);
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return string.Empty;
            }
        }

        public async Task SetEmailConfirmedAsync(ApplicationUser user)
        {
            try
            {
                user.EmailConfirmed = true;
                await _container.ReplaceItemAsync(user, user.Id.ToString());
            }
            catch (CosmosException ex)
            {
                var x = ex;
            }
        }

        public async Task<ApplicationUser> FindByIdAsync(Guid id)
        {
            try
            {
                var iterator = _container.GetItemLinqQueryable<ApplicationUser>(true)
                    .Where(u => u.Id == id).ToFeedIterator();
                var users = await iterator.ReadNextAsync();
                return users.FirstOrDefault();
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
        }

        public async Task<ApplicationUser> FindByNameAsync(string name, string providerName)
        {
            try
            {
                var iterator = _container.GetItemLinqQueryable<ApplicationUser>(true)
                    .Where(u => u.UserName == name &&
                    u.ProviderName == providerName).ToFeedIterator();
                var users = await iterator.ReadNextAsync();
                return users.FirstOrDefault();
            }
            catch (CosmosException ex)
            {
                var x = ex;
                return null;
            }
        }

        public async Task<ApplicationUser> FindByEmailAsync(string email, string providerName)
        {
            var iterator = _container.GetItemLinqQueryable<ApplicationUser>(true)
                    .Where(u => u.Email == email
                    && u.ProviderName == providerName).ToFeedIterator();
            var users = await iterator.ReadNextAsync();
            return users.FirstOrDefault();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _container.DeleteItemAsync<ApplicationUser>(id.ToString(), new PartitionKey(id.ToString()));
        }

        public async Task AcceptTermsOfUse(string email, string idp)
        {
            var user = await FindByEmailAsync(email, idp);
            user.HasAcceptedEula = true;
            await _container.ReplaceItemAsync<ApplicationUser>(user, user.Id.ToString());
        }
    }
}
