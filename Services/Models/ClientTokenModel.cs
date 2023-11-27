namespace MiniApp1Api.Services.Models;

public class ClientTokenModel
{
    public string AccessToken { get; set; }

    public DateTime AccessTokenExpiration { get; set; }
}