namespace FFVM.Manager.Services.Requests;

public class WriteAuthorizationRequest
{
    public string? Id { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
