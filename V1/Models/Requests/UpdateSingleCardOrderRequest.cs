namespace TransferProject.V1.Models.Requests;

public class UpdateSingleCardOrderRequest
{
    public string? CardName { get; set; }

    public string CardNumber { get; set; }

    public DateTime CardDate { get; set; }

    public string CVV { get; set; }

    public string UserId { get; set; }
}