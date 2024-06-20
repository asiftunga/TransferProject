using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransferProject.Data;
using TransferProject.Data.Entities;
using TransferProject.Data.Enums;
using TransferProject.Extensions;
using TransferProject.Filters;
using TransferProject.Services.Models;

namespace TransferProject.Services;

public class IdentityServer : IIdentityServer
{
    private readonly TransferProjectDbContext _transferProjectDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityServer(TransferProjectDbContext transferProjectDbContext, IHttpContextAccessor httpContextAccessor)
    {
        _transferProjectDbContext = transferProjectDbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async ValueTask<IdentityUserModel> GetAuthenticatedUser()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            ThrowUnauthorizedException();
        }

        string email = _httpContextAccessor.HttpContext.User.Identity.GetAuthenticatedUserEmail();

        string userId = _httpContextAccessor.HttpContext.User.Identity.GetAuthenticatedUserId();

        UserApp? user = await _transferProjectDbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);

        if (user == null)
        {
            ThrowBadRequestException();
        }

        UserApp? userFromId = await _transferProjectDbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

        if (userFromId == null)
        {
            ThrowBadRequestException();
        }

        if (user.Email != userFromId.Email)
        {
            ThrowUnauthorizedException();
        }

        if (user.Id != userId)
        {
            ThrowUnauthorizedException();
        }

        string role = _httpContextAccessor.HttpContext.User.Identity.GetAuthenticatedUserRole();

        IdentityUserModel identityUserModel = new();

        UserTypes userRole = (UserTypes)Enum.Parse(typeof(UserTypes), role);

        bool isAdmin = identityUserModel.HasRoleOfAdmin(userRole);

        identityUserModel.UserId = user.Id;
        identityUserModel.Email = user.Email;
        identityUserModel.Role = userRole;
        identityUserModel.IsAdmin = isAdmin;
        identityUserModel.User = user;

        return identityUserModel;
    }

    private static void ThrowUnauthorizedException()
    {
        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "unauthorized",
            Title = "Unauthorized",
            Detail = "Unauthorized Request!"
        };

        throw new ProblemDetailsException(problem);
    }

    private static void ThrowBadRequestException()
    {
        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status404NotFound,
            Type = "user-not-found.",
            Title = "User not found.",
            Detail = "User not found"
        };

        throw new ProblemDetailsException(problem);
    }
}