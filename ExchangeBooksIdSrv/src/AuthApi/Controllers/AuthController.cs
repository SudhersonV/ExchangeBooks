using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IdentityModel;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using static IdSrv.Infra.Constants;

namespace AuthApi
{
    [Route("mobileauth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        const string callbackScheme = "xamarinessentials";
        private readonly IConfiguration _configuration;
        ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("[action]/{scheme}")]
        public async Task Logout([FromRoute] string scheme)
        {
            var auth = await Request.HttpContext.AuthenticateAsync(scheme);
            if (auth?.Properties != null)
                await Request.HttpContext.SignOutAsync(scheme, auth?.Properties);
        }

        [HttpGet("{scheme}")]
        public async Task Get([FromRoute] string scheme)
        {
            var auth = await Request.HttpContext.AuthenticateAsync(scheme);

            if (!auth.Succeeded
                || auth?.Principal == null
                || !auth.Principal.Identities.Any(id => id.IsAuthenticated)
                || string.IsNullOrEmpty(auth?.Properties.GetTokenValue("access_token"))
                || string.IsNullOrEmpty(auth?.Properties.Items[".Token.expires_at"])
                || DateTime.Parse(auth.Properties.Items[".Token.expires_at"]).ToUniversalTime() <= DateTime.UtcNow.AddMinutes(5))
            {
                // Not authenticated, challenge
                await Request.HttpContext.ChallengeAsync(scheme);
            }
            else
            {
                //Log the auth
                // var settings = new JsonSerializerSettings();
                // settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                // var authString = JsonConvert.SerializeObject(auth.Principal.Claims, settings);
                // _logger.LogInformation($"logInfo:authString: {authString}");
                var qs = new Dictionary<string, string>();
                // Get parameters to send back to the callback
                var email = auth.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = auth.Principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value;
                var eulaAccepted = auth.Principal.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.EulaAccepted)?.Value;
                var idp = auth.Principal.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.Idp)?.Value;
                qs.Add(Constants.Email, email);
                qs.Add(Constants.Name, name);
                qs.Add(Constants.IdToken, auth.Properties.Items[".Token.id_token"]);
                qs.Add(Constants.AccessToken, auth.Properties.Items[".Token.access_token"]);
                qs.Add(Constants.RefreshToken, auth.Properties.Items[".Token.refresh_token"]);
                var accessTokenExpiresAtUTC = DateTime.Parse(auth.Properties.Items[".Token.expires_at"]).ToUniversalTime();
                qs.Add(Constants.AccessTokenExpiresAt, accessTokenExpiresAtUTC.ToString());
                qs.Add(Constants.RefreshTokenExpiresAt, DateTime.UtcNow.AddDays(Constants.RefreshTokenExpiresInDays).ToString());
                qs.Add(Constants.UserAcceptedEula, eulaAccepted);
                qs.Add(JwtClaimTypes.IdentityProvider, idp);
                // Build the result url
                var url = callbackScheme + "://#" + string.Join(
                    "&",
                    qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
                    .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                // Redirect to final url
                Request.HttpContext.Response.Redirect(url);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Invalid refresh token");

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", Constants.ClientId),
                new KeyValuePair<string, string>("client_secret", Constants.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            };
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_configuration["Authority"]);
                var response = await httpClient.PostAsync("connect/token", new FormUrlEncodedContent(pairs));

                if (!response.IsSuccessStatusCode)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var result = await response.Content.ReadAsStringAsync();
                var accessToken = JObject.Parse(result)["access_token"]?.ToString();
                var expiresInString = JObject.Parse(result)["expires_in"]?.ToString();
                var expiresAtUTC = DateTime.UtcNow.AddHours(long.Parse(expiresInString) / 3600).ToString();

                return Ok(new { access_token = accessToken, access_token_expires_at = expiresAtUTC });
            }
        }
    }
}
