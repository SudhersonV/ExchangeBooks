using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using IdSrv.Cosmos.Data.ConfigurationStore;
using IdentityServer4.Stores;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using AutoMapper;
using IdentityServer4.Services;
using IdSrv.Cosmos.Data.Services;
using IdentityServer4.Validation;
using IdSrv.Cosmos.Data.Entities;
using IdSrv.Cosmos.Data.Stores;
using IdSrv.Cosmos.Data.Interfaces;
using IdSrv.Infra.CustomTokenProvider;
using IdentityServer4;
using Microsoft.Azure.Cosmos;
using IdSrv.Infra;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;

namespace IdSrvHost.Web
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
            services.AddControllersWithViews();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddTransient<IClientStore, ClientStore>();
            services.AddTransient<IResourceStore, ResourceStore>();
            services.AddTransient<IApplicationUserStore<ApplicationUser>, ApplicationUserStore>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<ICosmosUserTwoFactorTokenProvider<ApplicationUser>, CustomEmailTokenProvider<ApplicationUser>>();
            services.AddTransient<ICosmosUserManager<ApplicationUser>, CosmosUserManager<ApplicationUser>>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IEmailService, EmailService>();
            services.Configure<CosmosDbOptions>(Configuration.GetSection("CosmosDb"));
            services.AddIdentityServer(options =>
            {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
            })
            .AddClientStore<ClientStore>()
            .AddResourceStore<ResourceStore>()
            .AddCorsPolicyService<CorsPolicyService>()
            .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
            .AddPersistedGrantStore<PersistedGrantStore>()
            .AddProfileService<ProfileService>()
            .AddDeveloperSigningCredential();
            services.AddAuthentication()
            .AddCookie(options =>
            {
                options.Cookie.Name = "exchangeBooksIdSrv";
            })
            .AddGoogle("Google", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = "";
                options.ClientSecret = "";
                options.SaveTokens = true;
            })
            .AddOpenIdConnect("apple", "Apple", async options =>
            {
                options.ResponseType = "code";
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.Scope.Clear(); // otherwise I had consent request issues
                options.Scope.Add("name");
                options.Scope.Add("email");
                //options.SaveTokens = true;
                options.Configuration = new OpenIdConnectConfiguration
                {
                    AuthorizationEndpoint = "https://appleid.apple.com/auth/authorize",
                    TokenEndpoint = "https://appleid.apple.com/auth/token",
                };
                options.ClientId = "com.sudhersonv.exchangebooksidsrv";
                options.CallbackPath = "/signin-apple";
                options.Events.OnAuthorizationCodeReceived = context =>
                {
                    context.TokenEndpointRequest.ClientSecret = CreateAppleToken();
                    return Task.CompletedTask;
                };
                // Expected identity token iss value
                options.TokenValidationParameters.ValidIssuer = "https://appleid.apple.com";
                // Expected identity token signing key
                var jwks = await new HttpClient().GetStringAsync("https://appleid.apple.com/auth/keys");
                options.TokenValidationParameters.IssuerSigningKey = new JsonWebKeySet(jwks).Keys.FirstOrDefault();
                // Disable nonce validation (not supported by Apple)
                options.ProtocolValidator.RequireNonce = false;
            });
            services.AddSingleton(s =>
            {
                var accountEndpoint = Configuration["CosmosDb:EndPointUrl"];
                var accountKey = Configuration["CosmosDb:PrimaryKey"];
                var configurationBuilder = new CosmosClientBuilder(accountEndpoint, accountKey)
                    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase });
                return configurationBuilder.Build();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseIdentityServer();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static string CreateAppleToken()
        {
            const string iss = "FLP5T3R986"; // your account's team ID found in the dev portal
            const string aud = "https://appleid.apple.com";
            const string sub = "com.sudhersonv.exchangebooksidsrv"; // same as client_id
            const string privateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQg1tJsPN3sQ+E4Zq49nSe4s2Yym7DUfYQIBRUJPRnb8gagCgYIKoZIzj0DAQehRANCAAQ6WbhN/6gcgVg+GmO1hRg+Ebi5eME7I5ZVzPME5/lr3h5h1gIrqJ+RKfm0Xqvp/ocbG3kUraPJf1hpfEN/46+F"; // contents of .p8 file

            var cngKey = CngKey.Import(
              Convert.FromBase64String(privateKey),
              CngKeyBlobFormat.Pkcs8PrivateBlob);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateJwtSecurityToken(
                issuer: iss,
                audience: aud,
                subject: new ClaimsIdentity(new List<Claim> { new Claim("sub", sub) }),
                expires: DateTime.UtcNow.AddMinutes(5), // expiry can be a maximum of 6 months
                issuedAt: DateTime.UtcNow,
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(
                  new ECDsaSecurityKey(new ECDsaCng(cngKey)), SecurityAlgorithms.EcdsaSha256));

            token.Header.Add("kid", "JS8Y9DXHS4");
            return handler.WriteToken(token);
        }
    }
}
