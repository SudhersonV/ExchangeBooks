using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using IdSrv.Cosmos.Data.Entities;
using IdSrv.Cosmos.Data.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdSrv.Infra.Constants;

namespace IdSrv.Cosmos.Data.Services
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly ICosmosUserManager<ApplicationUser> _cosmosUserManager;

        public ResourceOwnerPasswordValidator(ICosmosUserManager<ApplicationUser> cosmosUserManager)
        {
            _cosmosUserManager = cosmosUserManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            try
            {
                //get your user model from db (by UserName - in my case its email)
                var user = await _cosmosUserManager.FindByNameAsync(context.UserName);
                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, IdentityErrors.EmailNotConfirmed);
                        return;
                    }
                    //check if password match - remember to hash password if stored as hash in db
                    if (BCrypt.Net.BCrypt.Verify(context.Password, user.PasswordHash))
                    {
                        //set the result
                        context.Result = new GrantValidationResult(
                            subject: user.Email,
                            GrantType.ResourceOwnerPassword,
                            claims: GetUserClaims(user));
                        return;
                    }
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Incorrect password");
                    return;
                }
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User does not exist.");
                return;
            }
            catch (Exception ex)
            {
                var x = ex;
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid UserName or password");
            }
        }

        //build claims array from user data
        public static Claim[] GetUserClaims(ApplicationUser user)
        {
            var claims = new Claim[]
            {
            new Claim(JwtClaimTypes.Id, user.Id.ToString() ?? string.Empty),
            new Claim(JwtClaimTypes.Name, string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) ?
            string.Empty : $"{user.FirstName} {user.LastName}"),
            new Claim(JwtClaimTypes.GivenName, user.FirstName  ?? string.Empty),
            new Claim(JwtClaimTypes.FamilyName, user.LastName  ?? string.Empty),
            new Claim(JwtClaimTypes.Email, user.Email  ?? ""),
            new Claim(CustomClaimTypes.EulaAccepted, user.HasAcceptedEula.ToString(), "bool")
            };
            return claims;
        }
    }
}
