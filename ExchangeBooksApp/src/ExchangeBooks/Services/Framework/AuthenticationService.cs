using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Repository;
using ExchangeBooks.Models;
using Xamarin.Essentials;
using static ExchangeBooks.Constants.Constants;
using static ExchangeBooks.Constants.Constants.Auth;

namespace ExchangeBooks.Services.Framework
{
    public class AuthenticationService : IAuthenticationService
    {
        private const string UserName = "name";
        private const string UserEmail = "email";
        private const string AccessToken = "access_token";
        private const string AccessTokenExpiresAt = "access_token_expires_at";
        private const string RefreshToken = "refresh_token";
        private const string RefreshTokenExpiresAt = "refresh_token_expires_at";
        private const string FcmToken = "FcmToken";
        private const string FcmTokenSharedWithServer = "FcmTokenSharedWithServer";
        private const string BiometricsEnabled = "BiometricsEnabled";
        private const string UserAcceptedEula = "user_accepted_eula";
        private const string IdentityProvider = "idp";
        private Dictionary<string, string> AuthKeys;

        private readonly IDialogService _dialogService;
        private readonly IGenericRepository _repository;
        private readonly IEventTracker _eventTracker;

        public AuthenticationService(IDialogService dialogService, IGenericRepository repository
            , IEventTracker eventTracker)
        {
            _dialogService = dialogService;
            _repository = repository;
            _eventTracker = eventTracker;
            AuthKeys = new Dictionary<string, string> {
            { nameof(UserName), UserName }, { nameof(UserEmail), UserEmail }, { nameof(AccessToken), AccessToken },
            { nameof(AccessTokenExpiresAt), AccessTokenExpiresAt }, { nameof(RefreshToken), RefreshToken },
            { nameof(RefreshTokenExpiresAt), RefreshTokenExpiresAt }, { nameof(UserAcceptedEula), UserAcceptedEula },
            { nameof(IdentityProvider), IdentityProvider} };
        }

        #region Public Methods
        public async Task<bool> IsUserAuthenticated()
        {
            return await HasAccessToken();
        }

        public async Task<string> GetAccessToken()
        {
            if (await HasAccessToken())
                return await SecureStorage.GetAsync(nameof(AccessToken));
            else
                return string.Empty;
        }

        public async Task<string> GetUserName()
        {
            var userName = await SecureStorage.GetAsync(nameof(UserName));
            if (!string.IsNullOrEmpty(userName))
                return userName;
            else
                return string.Empty;
        }

        public async Task<string> GetUserEmail()
        {
            var userEmail = await SecureStorage.GetAsync(nameof(UserEmail));
            if (!string.IsNullOrEmpty(userEmail))
                return userEmail;
            else
                return string.Empty;
        }

        public async Task SetBiometricsFlag(bool flag)
        {
            await SecureStorage.SetAsync(nameof(BiometricsEnabled), flag.ToString());
        }

        public async Task Login()
        {
            try
            {
                WebAuthenticatorResult r = null;
                var authUrl = new Uri($"{Url}/{Paths.Login}");
                var callbackUrl = new Uri(RedirectUrl);
                r = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);
                AuthKeys.Keys.ToList().ForEach(async key =>
                {
                    var value = r?.Properties[AuthKeys[key]];
                    Console.WriteLine($"key {key} : value {value}");
                    await SecureStorage.SetAsync(key, WebUtility.UrlDecode(value));
                });
                _eventTracker.SendEvent("Authenticate", "AuthResult", "true");
                await RefreshFcmTokenAfterAuth();
            }
            catch (Exception ex)
            {
                _eventTracker.SendEvent("Authenticate", "AuthResult", "false");
                Console.WriteLine($"Error fetching access token, message: {ex?.Message}, innerExceptionMessage: {ex?.InnerException?.Message}");
            }
        }

        public async Task RefreshAccessToken()
        {
            try
            {
                var refreshToken = await SecureStorage.GetAsync(nameof(RefreshToken));
                var response = await _repository.PostAsync<string, RefreshTokenModel>($"{Url}/{Paths.Refresh}", refreshToken);
                await SecureStorage.SetAsync(nameof(AccessToken), response.Access_Token);
                await SecureStorage.SetAsync(nameof(AccessTokenExpiresAt), response.Access_Token_Expires_At);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
            }
        }

        public async Task<string> GetFcmToken()
        {
            return await SecureStorage.GetAsync(FcmToken);
        }

        public async Task UpdateFcmToken(string fcmToken)
        {
            var existingFcmToken = await SecureStorage.GetAsync(FcmToken);
            if (!string.IsNullOrEmpty(existingFcmToken) && string.Equals(existingFcmToken, fcmToken))
                return;
            await SecureStorage.SetAsync(FcmToken, fcmToken);
            await SecureStorage.SetAsync(FcmTokenSharedWithServer, "false");
            await RefreshFcmToken(fcmToken);
        }

        public void Logout()
        {
            SecureStorage.Remove(nameof(AccessToken));
            SecureStorage.Remove(nameof(AccessTokenExpiresAt));
        }

        public void ShowLoginSuccess()
        {
            _dialogService.Toast($"Logged in as {GetUserEmail()}!");
        }

        public async Task<bool> HasRefreshToken()
        {
            var refreshToken = await SecureStorage.GetAsync(nameof(RefreshToken));
            var refreshTokenExpiresAt = await SecureStorage.GetAsync(nameof(RefreshTokenExpiresAt));
            return !string.IsNullOrEmpty(refreshToken) && DateTime.TryParse(refreshTokenExpiresAt, out var result)
                && result > DateTime.UtcNow.AddMinutes(5);
        }
        public async Task<bool> IsBiometricsEnabled()
        {
            var value = await SecureStorage.GetAsync(nameof(BiometricsEnabled));
            return bool.TryParse(value, out bool result) ? result : false;
        }

        public async Task SetEulaAcceptance()
        {
            await SecureStorage.SetAsync(nameof(UserAcceptedEula), "True");
            await UpdateEulaAcceptance();
        }

        public async Task<bool> GetEulaAcceptance()
        {
            var value = await SecureStorage.GetAsync(nameof(UserAcceptedEula));
            return bool.TryParse(value, out bool result) ? result : false;
        }
        public async Task<string> GetIdentityProvider()
        {
            var value = await SecureStorage.GetAsync(nameof(IdentityProvider));
            return value ?? string.Empty;
        }
        #endregion

        #region Private Methods
        private async Task<bool> HasAccessToken()
        {
            var accessToken = await SecureStorage.GetAsync(nameof(AccessToken));
            var accessTokenExpiresAt = await SecureStorage.GetAsync(nameof(AccessTokenExpiresAt));
            return !string.IsNullOrEmpty(accessToken) && DateTime.TryParse(accessTokenExpiresAt, out var result)
                && result > DateTime.UtcNow.AddMinutes(5);
        }

        private async Task RefreshFcmTokenAfterAuth()
        {
            string fcmToken;
            if (!bool.TryParse(await SecureStorage.GetAsync(FcmTokenSharedWithServer), out bool IsFcmTokenShared)
                || IsFcmTokenShared || string.IsNullOrEmpty(fcmToken = await SecureStorage.GetAsync(FcmToken)))
                return;
            await RefreshFcmToken(fcmToken);
        }

        private async Task RefreshFcmToken(string fcmToken)
        {
            if (!await IsUserAuthenticated()) return;
            var userEmail = await GetUserEmail();
            try
            {
                await _repository.PostAsync($"{Api.Url}/{Api.Paths.Fcm.SetToken}", fcmToken, await GetAccessToken());
                await SecureStorage.SetAsync(FcmTokenSharedWithServer, "true");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                throw ex;
            }
        }

        private async Task UpdateEulaAcceptance()
        {
            try
            {
                await _repository.PatchAsync($"{Url}/{Paths.Eula}", string.Empty, await GetAccessToken());
                await SecureStorage.SetAsync(FcmTokenSharedWithServer, "true");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post, message: {ex.Message}, innerExceptionMessage: {ex.InnerException?.Message}");
                throw ex;
            }
        }
        #endregion
    }
}
