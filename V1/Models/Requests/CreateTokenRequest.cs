namespace MiniApp1Api.V1.Models.Requests;

public class CreateTokenRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}