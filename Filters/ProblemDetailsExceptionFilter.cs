using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TransferProject.Filters;

public class ProblemDetailsExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order { get; } = int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is ProblemDetailsException exception)
        {
            context.Result = new ObjectResult(exception.Value)
            {
                StatusCode = exception.Value.Status,
            };

            context.ExceptionHandled = true;
        }
    }
}