using System.Security;

namespace FFVM.Manager.Services.Requests;

public class GetAuthenticationTokenRequest(string userName, SecureString password)
{
    public string UserName { get; set; } = userName;
    public SecureString Password { get; set; } = password;
}
