using Microsoft.AspNetCore.Mvc;

namespace TransferProject.Filters;

public class ProblemDetailsException : Exception
{
    /// <summary>
    /// bu method private methodlarda problemdetails dondurmek icin kullanilir
    /// </summary>
    /// <param name="value"></param>
    public ProblemDetailsException(ProblemDetails value)
    {
        Value = value;
    }
    public ProblemDetails Value { get; }
}