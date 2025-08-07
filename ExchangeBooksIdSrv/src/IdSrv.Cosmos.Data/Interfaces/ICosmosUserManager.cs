using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IdSrv.Infra;

namespace IdSrv.Cosmos.Data.Interfaces
{
    public interface ICosmosUserManager<T>
    {
        Task<T> FindByNameAsync(string name, string providerName = Constants.ExchangeBooks);
        Task<T> FindByIdAsync(Guid id);
        Task<T> FindByEmailAsync(string email, string providerName = Constants.ExchangeBooks);
        Task<(HttpStatusCode, T)>CreateUserAsync(T user);
        Task<Guid> GetUserIdAsync(T user);
        Task<byte[]> CreateSecurityTokenAsync(T user);        
        Task<bool> IsEmailConfirmedAsync(T user);
        Task SetEmailConfirmedAsync(T user);        
        Task<string> GetEmailAsync(T user);
        Task<string> GenerateEmailConfirmationTokenAsync(T user);
        Task<bool> ConfirmEmailAsync(T user, string token);
        Task AcceptTermsOfUse(string email, string idp);
    }
}
