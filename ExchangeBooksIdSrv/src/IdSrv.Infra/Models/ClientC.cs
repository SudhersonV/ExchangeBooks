using System;
using IdentityServer4.Models;

namespace IdSrv.Infra.Models
{
    public class ClientC: Client
    {
        public Guid Id { get; set; }
    }
}