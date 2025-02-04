namespace FFVM.Base.Config;

public class AuthorizationRecord(string id, DateTime expiresAt, string? token = null)
{
    public string Id { get; set; } = id;
    public string? Token { get; set; } = token;
    public DateTime ExpiresAt { get; set; } = expiresAt;
}
