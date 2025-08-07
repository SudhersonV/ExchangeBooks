using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.Interfaces
{
    public interface IApplicationUserStore<T>
    {
        Task<T> FindByNameAsync(string name, string providerName);
        Task<T> FindByEmailAsync(string email, string providerName);
        Task<T> FindByIdAsync(Guid id);
        Task DeleteUserAsync(Guid id);
        Task<(HttpStatusCode, T)> CreateUserAsync(T user);
        Task<string> GetSecurityStampAsync(T user);
        Task SetSecurityStampAsync(T user, string stamp);
        Task<Guid> GetUserIdAsync(T user);
        Task<bool> IsEmailConfirmedAsync(T user);
        Task SetEmailConfirmedAsync(T user);
        Task<string> GetEmailAsync(T user);
        Task<T> FindByExternalProvider(string provider, string userId);
        Task<T> AutoProvisionUser(string provider, string userId, List<Claim> claims);
        Task AcceptTermsOfUse(string email, string idp);
    }
}
