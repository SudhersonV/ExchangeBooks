using System;
using System.Collections.Generic;
using System.Text;

namespace IdSrv.Infra
{
    public class Constants
    {
        public const string ExchangeBooks = "local";
        public class IdentityErrors
        {
            public const string EmailNotConfirmed = "Email not confirmed";
        }
        public class CustomClaimTypes
        {
            public const string EulaAccepted = "EulaAccepted";

            public const string Idp = "http://schemas.microsoft.com/identity/claims/identityprovider";
        }
    }
}
