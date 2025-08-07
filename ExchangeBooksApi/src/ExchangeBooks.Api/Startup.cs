using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4.AccessTokenValidation;
using static ExchangeBooks.Infra.Constants;
using Microsoft.Azure.Cosmos.Fluent;
using ExchangeBooks.Infra.Interfaces;
using ExchangeBooks.Services;
using AutoMapper;
using System;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Azure.Cosmos;

namespace ExchangeBooks.Api
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
            services.AddControllers();
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.GetApplicationDefault(),
            });
            // FirebaseApp.Create();
            #region JWT Token Auth
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            // .AddJwtBearer(options =>
            // {
            //         // base-address of your identityserver
            //         options.Authority = Constants.IdServer.Authority;
            //         // name of the API resource
            //         options.Audience = "customAPI";
            //     options.RequireHttpsMetadata = true;
            //     options.SaveToken = true;
            // });
            #endregion
            #region Reference Tken Auth
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddIdentityServerAuthentication(options =>
            {
                // base-address of your identityserver
                options.Authority = Configuration["Authority"];
                // name of the API resource
                options.ApiName = "exchangeBooksApi";
                options.ApiSecret = "secret";
            });
            #endregion
            services.AddAuthorization(options =>
            {
                options.AddPolicy("checkwritescope", builder =>
                {
                    builder.RequireAuthenticatedUser();
                    builder.RequireScope(new string[] { "exchangeBooks.write" });
                });
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddHttpContextAccessor();
            services.AddScoped<IClaimsAccessor, ClaimsAccessor>();
            services.AddScoped<IPostsService, PostsService>();
            services.AddScoped<IMessagesService, MessagesService>();
            services.AddScoped<IFcmService, FcmService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IEmailService, EmailService>();
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
