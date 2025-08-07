using IdentityServer4.Models;
using System;

namespace IdSrv.Infra.Models
{
    public class ApiResourceC: ApiResource
    {
        public Guid Id { get; set; }
    }
}