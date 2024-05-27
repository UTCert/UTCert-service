namespace UTCert.Model.Shared.Authorization;

public class AppSettings
{
    public string Secret { get; set; }
    public int RefreshTokenTTL { get; set; }
}