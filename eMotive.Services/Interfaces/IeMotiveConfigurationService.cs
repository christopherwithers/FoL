namespace eMotive.Services.Interfaces
{
    public interface IeMotiveConfigurationService
    {
        string PusherID();
        string PusherKey();
        string PusherSecret();

        string EmailFromAddress();
        bool EmailsEnabled();

        int MaxLoginAttempts();
        int LockoutTimeInMinutes();

        string SiteName();
        string SiteURL();

        string GetClientIpAddress();
    }
}
