using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ExchangeBooks.Infra;
using ExchangeBooks.Infra.Interfaces;

namespace ExchangeBooks.Services
{
    public class ClaimsAccessor : IClaimsAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClaimsAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Name => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;

        public string Email => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value.ToLower();

        public string MobilePhone => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;

        public string AppHostUrl
        {
            get
            {
                var request = _httpContextAccessor.HttpContext.Request;
                var url = $"{ request.Scheme}://{request.Host.Host}:{request.Host.Port}";
                return url;
            }
        }
        public string AccessToken => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AccessToken)?.Value;
    }
}
