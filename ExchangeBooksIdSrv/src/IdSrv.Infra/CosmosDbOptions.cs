namespace IdSrv.Infra
{
    public class CosmosDbOptions
    {
        public string DatabaseId { get; set; }
        public string ApiResourcesContainerId { get; set; }
        public string ApplicationUsersContainerId { get; set; }
        public string ClientsContainerId { get; set; }
        public string IdentityResourcesContainerId { get; set; }
        public string PersistedGrantsContainerId { get; set; }
    }
}