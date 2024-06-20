namespace TransferProject.V1.Models.Requests;

public class CreateTokenByClientRequest
{
    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}