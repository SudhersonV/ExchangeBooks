using System;
using IdentityServer4.Models;

namespace IdSrv.Infra.Models
{
    public class IdentityResourceC: IdentityResource
    {
        public Guid Id { get; set; }
    }
}