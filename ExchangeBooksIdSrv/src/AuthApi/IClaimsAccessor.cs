namespace AuthApi
{
    public interface IClaimsAccessor
    {
        string Name { get; }

        string Email { get; }

        string MobilePhone { get; }

        string AppHostUrl { get; }

        string AccessToken { get; }

        string Idp { get; }
    }
}