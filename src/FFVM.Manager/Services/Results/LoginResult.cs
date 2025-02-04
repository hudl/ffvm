namespace FFVM.Manager.Services.Results;

public class LoginResult<TCredentialsType>
{
    public DateTime VerifiedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public TCredentialsType? CredentialsContainer { get; set; }
}
