using AutoMapper;
using IdentityServer4.Models;
using IdSrv.Cosmos.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdSrv.Cosmos.Data
{
    public class CosmosIdSrvMap : Profile
    {
        public CosmosIdSrvMap()
        {
            CreateMap<PersistedGrant, PersistedGrantEntity>();
            CreateMap<PersistedGrantEntity, PersistedGrant>();
        }
    }
}
