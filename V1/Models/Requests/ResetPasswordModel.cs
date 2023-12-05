namespace MiniApp1Api.V1.Models.Requests;

public class ResetPasswordModel
{
    public string Email { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}