namespace MiniApp1Api.V1.Models.Requests;

public class UpdateSingleCardOrderRequest
{
    public string? CardName { get; set; }

    public string CardNumber { get; set; }

    public DateTime CardDate { get; set; }

    public string CVV { get; set; }
}