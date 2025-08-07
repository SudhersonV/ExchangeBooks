using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdSrv.Infra.InMemoryModels
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client> {
                new Client {
                    ClientId = "oauthClient",
                    ClientName = "Example Client Credentials Client Application",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = new List<Secret> {
                        new Secret("superSecretPassword".Sha256())},
                    AllowedScopes = new List<string> {"customAPI.read"}
                },
                new Client {
                    ClientId = "mvcImplicitClient",
                    ClientName = "Example Implicit Client Application",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "role",
                        "customAPI.write"
                    },
                    RedirectUris = new List<string> {"http://localhost:44330/signin-oidc"},
                    PostLogoutRedirectUris = new List<string> {"http://localhost:44330"}
                },
                new Client {
                    ClientId = "mvcCodeFlowClient",
                    ClientName = "Example Code Flow Client Application",
                    ClientSecrets = new List<Secret> {
                        new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "role",
                        "customAPI.read"
                    },
                    RedirectUris = new List<string> {"http://localhost:58561/signin-oidc"},
                    PostLogoutRedirectUris = { "http://localhost:58561/signout-callback-oidc" },
                    AllowRememberConsent = false,                      
                },
                new Client {
                    ClientId = "ExchangeBooksXamarin",
                    ClientName = "Resell Books Xamarin Mobile App",
                    ClientSecrets = new List<Secret> {
                        new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "customAPI.read",
                        "customAPI.write"
                    },
                    RedirectUris = new List<string> {"http://localhost:58561/signin-oidc"},
                    PostLogoutRedirectUris = { "http://localhost:58561/signout-callback-oidc" },
                    AllowRememberConsent = false           
                }
            };
        }
    }
}