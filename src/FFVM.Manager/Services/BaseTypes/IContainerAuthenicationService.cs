using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;

namespace FFVM.Manager.Services.BaseTypes;

public interface IContainerAuthenicationService<TAuthenticationType, TCredentialsType>
{
    Task<LoginResult<TCredentialsType>?> Login(LoginRequest<TAuthenticationType> loginRequest);
}
