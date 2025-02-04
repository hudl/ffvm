using RestSharp;
using RestSharp.Authenticators;

namespace FFVM.Manager.Utility;

public class DockerHubRestSharpAuthenticator(string token) : AuthenticatorBase(token)
{
    protected override ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
    {
        return ValueTask.FromResult(new HeaderParameter(KnownHeaders.Authorization, $"Bearer {Token}") as Parameter);
    }
}
