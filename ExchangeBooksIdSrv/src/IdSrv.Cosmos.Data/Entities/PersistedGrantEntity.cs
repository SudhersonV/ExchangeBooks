using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdSrv.Cosmos.Data.Entities
{
    class PersistedGrantEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Key;

        [JsonProperty(PropertyName = "type")]
        public string Type;

        [JsonProperty(PropertyName = "subject_id")]
        public string SubjectId;

        [JsonProperty(PropertyName = "client_id")]
        public string ClientId;

        [JsonProperty(PropertyName = "creation_time")]
        public DateTime CreationTime;

        [JsonProperty(PropertyName = "expiration")]
        public DateTime? Expiration;

        [JsonProperty(PropertyName = "data")]
        public string Data;
    }
}
