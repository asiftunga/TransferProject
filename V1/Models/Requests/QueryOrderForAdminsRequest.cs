using MiniApp1Api.Data.Enums;
using TransferProject.Models;

namespace MiniApp1Api.V1.Models.Requests;

public class QueryOrderForAdminsRequest : PagedRequest
{
    public int? AmountStart { get; set; }

    public int? AmountEnd { get; set; }

    public string? UserId { get; set; }

    public OrderTypes? OrderType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public OrderStatus? OrderStatus { get; set; }

    public Currencys? Currency { get; set; }
}