using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IdSrvClient.Infra
{
    public class ClaimsAccessor : IClaimsAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClaimsAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Name => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        public string Email => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

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
        public string AccessTokenExpiresAt => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AccessTokenExpiresAt)?.Value;
        public string RefreshToken => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.RefreshToken)?.Value;
        public string IdToken => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.IdToken)?.Value;
    }
}
