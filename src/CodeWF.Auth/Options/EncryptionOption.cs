namespace CodeWF.Auth.Options;

public class EncryptionOption
{
    public string PasswordSalt { get; set; } = null!;

    public string Account { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}