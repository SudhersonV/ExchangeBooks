namespace IdSrvClient.Infra
{
    public interface IClaimsAccessor
    {
        string Name { get; }

        string Email { get; }

        string MobilePhone { get; }

        string AppHostUrl { get; }

        string AccessToken { get; }
        string AccessTokenExpiresAt { get; }
        string RefreshToken { get; }
        string IdToken { get; }
    }
}