namespace TransferProject.Services.Models;

public class TokenModel
{
    public string AccessToken { get; set; }

    public DateTime AccessTokenExpiration { get; set; }

    public string RefreshToken { get; set; }

    public DateTime RefreshTokenExpiration { get; set; }
}