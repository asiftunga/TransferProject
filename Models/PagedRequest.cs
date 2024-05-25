namespace TransferProject.Models;

public class PagedRequest
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public string OrderBy { get; set; }

    public PaginationOrderType Order { get; set; }
}

public enum PaginationOrderType
{
    Unknown,
    Asc = 1,
    Desc = 2
}