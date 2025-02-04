namespace FFVM.Manager.Services.Requests;

public class LoginRequest<TAuthenticationType>
{
    public TAuthenticationType? Authentication { get; set; }
}
