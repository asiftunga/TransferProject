namespace MiniApp1Api.Data.Entities;

public class Token
{
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public DateTime AccessTokenExpiration { get; set; }

    public DateTime RefreshTokenExpiration { get; set; }
}