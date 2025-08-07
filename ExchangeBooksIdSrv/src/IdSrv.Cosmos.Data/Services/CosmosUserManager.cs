using IdSrv.Cosmos.Data.Entities;
using IdSrv.Cosmos.Data.Interfaces;
using IdSrv.Infra;
using Serenity.Data;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.Services
{
    public class CosmosUserManager<TUser> : ICosmosUserManager<TUser>
    {
        #region Variables
        private readonly IApplicationUserStore<TUser> _applicationUserStore;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private readonly ICosmosUserTwoFactorTokenProvider<TUser> _tokenProvider;
        public const string ConfirmEmailTokenPurpose = "EmailConfirmation";
        #endregion
        #region Constructor
        public CosmosUserManager(IApplicationUserStore<TUser> applicationUserStore,
            ICosmosUserTwoFactorTokenProvider<TUser> tokenProvider)
        {
            _applicationUserStore = applicationUserStore;
            _tokenProvider = tokenProvider;
        }
        #endregion
        #region Public Methods
        public async Task<byte[]> CreateSecurityTokenAsync(TUser user)
        {
            return Encoding.Unicode.GetBytes(await GetSecurityStampAsync(user));
        }
        public async Task<string> GetEmailAsync(TUser user)
        {
            return await _applicationUserStore.GetEmailAsync(user);
        }
        public async Task<Guid> GetUserIdAsync(TUser user)
        {
            return await _applicationUserStore.GetUserIdAsync(user);
        }
        public async Task<bool> IsEmailConfirmedAsync(TUser user)
        {
            return await _applicationUserStore.IsEmailConfirmedAsync(user);
        }
        public async Task SetEmailConfirmedAsync(TUser user)
        {
            await _applicationUserStore.SetEmailConfirmedAsync(user);
            await UpdateSecurityStampInternal(user);
        }
        public async Task<TUser> FindByNameAsync(string name, string providerName = Constants.ExchangeBooks)
        {
            return await _applicationUserStore.FindByNameAsync(name, providerName);
        }
        public async Task<TUser> FindByEmailAsync(string email, string providerName = Constants.ExchangeBooks)
        {
            return await _applicationUserStore.FindByEmailAsync(email, providerName);
        }
        public async Task<TUser> FindByIdAsync(Guid id)
        {
            return await _applicationUserStore.FindByIdAsync(id);
        }
        public async Task<(HttpStatusCode, TUser)> CreateUserAsync(TUser user)
        {
            var cosmosUser = await _applicationUserStore.CreateUserAsync(user);
            await UpdateSecurityStampInternal(user);
            return cosmosUser;
        }
        public async Task<string> GenerateEmailConfirmationTokenAsync(TUser user)
        {
            return await _tokenProvider.GenerateAsync(ConfirmEmailTokenPurpose, this, user);
        }
        public async Task<bool> ConfirmEmailAsync(TUser user, string token)
        {
            if (!await _tokenProvider.ValidateAsync(ConfirmEmailTokenPurpose, token, this, user))
            {
                return false;
            }
            await SetEmailConfirmedAsync(user);
            return true;
        }

        public async Task AcceptTermsOfUse(string email, string idp)
        {
            await _applicationUserStore.AcceptTermsOfUse(email, idp);
        }
        #endregion
        #region Private Methods
        private async Task<string> GetSecurityStampAsync(TUser user)
        {
            return await _applicationUserStore.GetSecurityStampAsync(user);
        }
        // Update the security stamp if the store supports it
        private async Task UpdateSecurityStampInternal(TUser user)
        {
            await _applicationUserStore.SetSecurityStampAsync(user, NewSecurityStamp());
        }
        private static string NewSecurityStamp()
        {
            byte[] bytes = new byte[20];
            _rng.GetBytes(bytes);
            return Base32.Encode(bytes);
        }
        #endregion
    }
}
