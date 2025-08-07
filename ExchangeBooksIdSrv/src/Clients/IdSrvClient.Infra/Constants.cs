using System;

namespace IdSrvClient.Infra
{
    public class Constants
    {
        // public const string Authority = "https://localhost:6001/";
        public const string Authority = "https://dev-exchangebooksidentityserver.azurewebsites.net";
        // public const string Authority = "https://exchangebooksidentityserver.azurewebsites.net";
        public const string ClientId = "mvcCodeFlowClient";
        public const string ClientSecret = "secret";
        public class ClaimTypes
        {
            public const string IdToken = "idtoken";
            public const string AccessToken = "accesstoken";
            public const string RefreshToken = "refreshtoken";
            public const string AccessTokenExpiresAt = "accesstokenexpiresat";
        }
    }
}
