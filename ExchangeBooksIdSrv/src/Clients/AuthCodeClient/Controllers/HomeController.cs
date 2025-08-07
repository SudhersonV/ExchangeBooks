using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthCodeClient.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using IdentityServer4.Services;
using IdSrvClient.Infra;
using AuthCodeClient.ViewModels;
using IdentityModel.OidcClient;
using System.Web;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace AuthCodeClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClaimsAccessor _claimsAccessor;

        public HomeController(ILogger<HomeController> logger, IClaimsAccessor claimsAccessor)
        {
            _logger = logger;
            _claimsAccessor = claimsAccessor;
        }

        /// <summary>
        /// No Authentication required
        /// </summary>
        /// <returns></returns>
        public IActionResult PublicPage()
        {
            return View();
        }

        [Authorize]
        public IActionResult AuthenticationOnly()
        {
            var claims = new List<string>{
                $"{nameof(_claimsAccessor.AccessToken)}: {_claimsAccessor.AccessToken}",
                $"{nameof(_claimsAccessor.AccessTokenExpiresAt)}: {_claimsAccessor.AccessTokenExpiresAt}",
                $"{nameof(_claimsAccessor.RefreshToken)}: {_claimsAccessor.RefreshToken}",
                $"{nameof(_claimsAccessor.IdToken)}: {_claimsAccessor.IdToken}",
            };
            return View(claims);
        }

        [Authorize]
        public async Task<IActionResult> AuthorizationIncluded()
        {
            var accesstoken = _claimsAccessor.AccessToken;
            var result = new List<WeatherForecast>();
            # region Call web api to get weather data
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7001");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
                var response = await client.GetAsync("weatherforecast");
                if (response.IsSuccessStatusCode)
                    result = await response.Content.ReadAsAsync<List<WeatherForecast>>();
            }
            #endregion
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Refresh(string refreshToken)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.Authority);
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", Constants.ClientId),
                new KeyValuePair<string, string>("client_secret", Constants.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            };
            var response = await client.PostAsync("connect/token", new FormUrlEncodedContent(pairs));
            var result = await response.Content.ReadAsStringAsync();
            var principal = Request.HttpContext.User;
            var claimsIdentity = new ClaimsIdentity(principal.Identity);
            var expiresInString = JObject.Parse(result)["expires_in"]?.ToString();
            var expiresIn = DateTime.UtcNow.AddHours(long.Parse(expiresInString)/3600);
            claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.AccessToken, JObject.Parse(result)["access_token"]?.ToString(), "string"));
            claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.AccessTokenExpiresAt, expiresIn.ToString(), "string"));
            claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.RefreshToken, JObject.Parse(result)["refresh_token"]?.ToString(), "string"));
            claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.IdToken, JObject.Parse(result)["id_token"]?.ToString(), "string"));
            principal = new ClaimsPrincipal(new ClaimsIdentity[] { claimsIdentity });
            var claims = new List<string>{
                $"{nameof(_claimsAccessor.AccessToken)}: {JObject.Parse(result)["access_token"]?.ToString()}",
                $"{nameof(_claimsAccessor.AccessTokenExpiresAt)}: {expiresIn.ToString()}",
                $"{nameof(_claimsAccessor.RefreshToken)}: {JObject.Parse(result)["refresh_token"]?.ToString()}",
                $"{nameof(_claimsAccessor.IdToken)}: {JObject.Parse(result)["id_token"]?.ToString()}",
            };
            return View(claims);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
