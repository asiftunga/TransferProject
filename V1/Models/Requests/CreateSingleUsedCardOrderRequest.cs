using TransferProject.Data.Enums;

namespace TransferProject.V1.Models.Requests;

public class CreateSingleUsedCardOrderRequest
{
    public Guid OrderId { get; set; }

    public OrderTypes OrderType { get; set; }

    public int Amount { get; set; }

    public Currencys Currency { get; set; }

    public PaymentMethods PaymentMethod { get; set; }

    public PaymentAreas? PaymentArea { get; set; }
}