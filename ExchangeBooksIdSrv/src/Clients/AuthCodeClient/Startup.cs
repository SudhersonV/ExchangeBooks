using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using IdSrvClient.Infra;
using System;

namespace AuthCodeClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            services.AddScoped<IClaimsAccessor, ClaimsAccessor>();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie(options =>
                {
                    options.Cookie.Name = "mvccode";
                })
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = Constants.Authority;
                options.RequireHttpsMetadata = true;
                options.ClientId = Constants.ClientId;
                options.ClientSecret = Constants.ClientSecret;
                // code flow + PKCE (PKCE is turned on by default)
                options.ResponseType = "code";
                options.UsePkce = true;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("exchangeBooks.write");
                //options.Scope.Add("role");
                options.Scope.Add("offline_access");

                // not mapped by default
                options.ClaimActions.MapJsonKey("website", "website");

                // keeps id_token smaller                
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role,
                };
                options.Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = async n =>
                    {
                        var claimsIdentity = new ClaimsIdentity(n.Principal.Identity);
                        var accessToken = n.Properties.Items[".Token.access_token"];
                        claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.AccessToken, accessToken, "string"));
                        claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.AccessTokenExpiresAt, DateTime.Parse(n.Properties.Items[".Token.expires_at"]).ToUniversalTime().ToString(), "string"));
                        claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.RefreshToken, n.Properties.Items[".Token.refresh_token"], "string"));
                        claimsIdentity.AddClaim(new Claim(Constants.ClaimTypes.IdToken, n.Properties.Items[".Token.id_token"], "string"));
                        n.Principal = new ClaimsPrincipal(new ClaimsIdentity[] { claimsIdentity });
                        await Task.FromResult(accessToken);
                    },
                    OnRemoteFailure = async n =>
                    {
                        await Task.FromResult(n);
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=PublicPage}/{id?}");
            });
        }
    }
}
