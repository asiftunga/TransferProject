using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using TransferProject.Filters;

namespace TransferProject.Extensions;

public static class IdentityExtension
{
    public static string GetAuthenticatedUserEmail(this IIdentity identity)
    {
        string? email = identity.FindFirstOrEmpty(ClaimTypes.Email);

        if (email != null)
        {
            return email;
        }

        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "unauthorized",
            Title = "Unauthorized",
            Detail = "Unauthorized Request!"
        };

        throw new ProblemDetailsException(problem);
    }

    public static string GetAuthenticatedUserId(this IIdentity identity)
    {
        string? Id = identity.FindFirstOrEmpty(ClaimTypes.NameIdentifier);

        if (Id != null)
        {
            return Id;
        }

        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "unauthorized",
            Title = "Unauthorized",
            Detail = "Unauthorized Request!"
        };

        throw new ProblemDetailsException(problem);
    }

    public static string GetAuthenticatedUserRole(this IIdentity identity)
    {
        string? role = identity.FindFirstOrEmpty(ClaimTypes.Role);

        if (role != null)
        {
            return role;
        }

        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "unauthorized",
            Title = "Unauthorized",
            Detail = "Unauthorized Request!"
        };

        throw new ProblemDetailsException(problem);
    }

    private static string FindFirstOrEmpty(this IIdentity identity, string type)
    {
        if (identity is not ClaimsIdentity claimsIdentity)
        {
            throw new Exception("Token is not valid!");
        }

        Claim claim = claimsIdentity.FindFirst(type);

        return claim?.Value;
    }
}