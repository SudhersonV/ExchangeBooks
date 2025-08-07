using System;
using AutoMapper;
using IdentityServer4.AccessTokenValidation;
using IdSrv.Cosmos.Data.Entities;
using IdSrv.Cosmos.Data.Interfaces;
using IdSrv.Cosmos.Data.Services;
using IdSrv.Cosmos.Data.Stores;
using IdSrv.Infra;
using IdSrv.Infra.CustomTokenProvider;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie(options =>
            {
                options.Cookie.Name = "exchangeBooksWebAuth";
                options.ExpireTimeSpan = new System.TimeSpan(0, 0, 0);
            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = Configuration["Authority"];
                options.RequireHttpsMetadata = true;
                options.ClientId = Constants.ClientId;
                options.ClientSecret = Constants.ClientSecret;
                options.ResponseType = "code";
                options.UsePkce = true;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("exchangeBooks.write");
                options.Scope.Add("offline_access");
                options.SaveTokens = true;
            })
            #region Reference Token Auth
            .AddIdentityServerAuthentication(options =>
            {
                // base-address of your identityserver
                options.Authority = Configuration["Authority"];
                // name of the API resource
                options.ApiName = "exchangeBooksApi";
                options.ApiSecret = "secret";
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("checkwritescope", builder =>
                {
                    builder.RequireAuthenticatedUser();
                    builder.RequireScope(new string[] { "exchangeBooks.write" });
                    builder.AddAuthenticationSchemes(IdentityServerAuthenticationDefaults.AuthenticationScheme);
                });
            });
            #endregion
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddTransient<IApplicationUserStore<ApplicationUser>, ApplicationUserStore>();
            services.AddTransient<ICosmosUserManager<ApplicationUser>, CosmosUserManager<ApplicationUser>>();
            services.AddTransient<ICosmosUserTwoFactorTokenProvider<ApplicationUser>, CustomEmailTokenProvider<ApplicationUser>>();
            services.AddHttpContextAccessor();
            services.AddScoped<IClaimsAccessor, ClaimsAccessor>();
            services.Configure<CosmosDbOptions>(Configuration.GetSection("CosmosDb"));
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

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
