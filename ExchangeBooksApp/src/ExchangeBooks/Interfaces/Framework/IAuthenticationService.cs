using System;
using System.Threading.Tasks;

namespace ExchangeBooks.Interfaces.Framework
{
    public interface IAuthenticationService
    {
        Task<bool> IsUserAuthenticated();
        Task<string> GetAccessToken();
        Task<string> GetUserName();
        Task<string> GetUserEmail();
        Task SetBiometricsFlag(bool flag);
        Task Login();
        Task RefreshAccessToken();
        Task<bool> HasRefreshToken();
        Task<bool> IsBiometricsEnabled();
        void Logout();
        Task<string> GetFcmToken();
        Task UpdateFcmToken(string token);
        void ShowLoginSuccess();
        Task SetEulaAcceptance();
        Task<bool> GetEulaAcceptance();
        Task<string> GetIdentityProvider();
    }
}
