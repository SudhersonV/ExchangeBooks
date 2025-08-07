using IdentityServer4.Models;
using IdentityServer4.Services;
using IdSrv.Cosmos.Data.Entities;
using IdSrv.Cosmos.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.Services
{
    public class ProfileService : IProfileService
    {
        //services
        private readonly ICosmosUserManager<ApplicationUser> _cosmosUserManager;

        public ProfileService(ICosmosUserManager<ApplicationUser> cosmosUserManager)
        {
            _cosmosUserManager = cosmosUserManager;
        }

        //Get user profile date in terms of claims when calling /connect/userinfo
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                //depending on the scope accessing the user data.
                if (!string.IsNullOrEmpty(context.Subject.Identity.Name))
                {
                    var providerName = context.Subject.Claims.FirstOrDefault(x => x.Type == "idp")?.Value;
                    //get user from db (in my case this is by email)
                    var user = await _cosmosUserManager.FindByNameAsync(context.Subject.Identity.Name, providerName);

                    if (user != null)
                    {
                        var claims = ResourceOwnerPasswordValidator.GetUserClaims(user);
                        //set issued claims to return
                        //context.IssuedClaims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
                        context.IssuedClaims = claims.ToList();
                    }
                }
                else
                {
                    //get subject from context (this was set ResourceOwnerPasswordValidator.ValidateAsync),
                    //where and subject was set to my user id.
                    var userId = context.Subject.Claims.FirstOrDefault(x => x.Type == "sub");

                    if (!userId.ToString().Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //get user from db (find user by user id)
                        var user = await _cosmosUserManager.FindByIdAsync(new Guid(userId.Value));

                        // issue the claims for the user
                        if (user != null)
                        {
                            var claims = ResourceOwnerPasswordValidator.GetUserClaims(user);
                            //context.IssuedClaims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
                            context.IssuedClaims = claims.ToList();
                        }
                    }
                }
            }
            catch{}
        }

        //check if user account is active.
        public async Task IsActiveAsync(IsActiveContext context)
        {
            try
            {
                //get subject from context (set in ResourceOwnerPasswordValidator.ValidateAsync),
                var userId = context.Subject.Claims.FirstOrDefault(x => x.Type == "sub");
                var providerName = context.Subject.Claims.FirstOrDefault(x => x.Type == "idp")?.Value;
                if (!string.IsNullOrEmpty(userId?.Value) && !(userId.ToString().Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    var user = await _cosmosUserManager.FindByNameAsync(userId.Value, providerName);
                    context.IsActive = user != null;
                }
            }
            catch (Exception ex)
            {
                var x = ex;
                //handle error logging
            }
        }
    }
}
