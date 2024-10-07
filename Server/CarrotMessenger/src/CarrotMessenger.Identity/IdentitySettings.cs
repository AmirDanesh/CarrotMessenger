namespace CarrotMessenger.Identity;

public class IdentitySettings
{
    public required string Issuer { get; set; }

    public required TimeSpan RefreshExpiresIn { get; set; }
    public required TimeSpan ExpiresIn { get; set; }
    public required string[] Audiences { get; set; }
    public required string Secret { get; set; }
    public bool Save { get; set; }
}