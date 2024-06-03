using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.Http;
using MiniApp1Api.BackgroundServices;
using MiniApp1Api.Data;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.Data.Enums;
using MiniApp1Api.Data.Identity;
using MiniApp1Api.Services;
using MiniApp1Api.Services.Models;
using MiniApp1Api.V1.Models.Requests;
using MiniApp1Api.V1.Models.Responses;

namespace MiniApp1Api.V1.Controllers;


[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/[controller]/[action]")]
public class OrderV1Controller : ControllerBase
{
    private readonly CustomUserManager<UserApp> _userManager;
    private readonly ForgotPasswordEmailSenderBackgroundService _forgotPasswordEmailSenderService;
    private readonly TransferProjectDbContext _transferProjectDbContext;
    private readonly IIdentityServer _identityServer;

    public OrderV1Controller(
        CustomUserManager<UserApp> userManager,
        ForgotPasswordEmailSenderBackgroundService forgotPasswordEmailSenderService,
        TransferProjectDbContext transferProjectDbContext, IIdentityServer identityServer)
    {
        _userManager = userManager;
        _forgotPasswordEmailSenderService = forgotPasswordEmailSenderService;
        _transferProjectDbContext = transferProjectDbContext;
        _identityServer = identityServer;
    }

    [HttpHead]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApprovedInfo()
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        bool anyUnreadMessage = await _transferProjectDbContext.ApprovedOrders.AsNoTracking().AnyAsync(x => x.UserId == userModel.UserId && x.IsRead);

        HttpContext.Response.Headers.Add("X-IsAnyUnreadMessages", anyUnreadMessage.ToString());

        return NoContent();
    }

    [HttpGet("{orderType:int}")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderInfo([FromRoute(Name = "orderType")] int orderType)
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        Guid orderId = Guid.NewGuid();

        TemporaryOrder temporaryOrder = new()
        {
            UserId = Guid.Parse(userModel.UserId),
            OrderId = orderId,
            OrderType = (OrderTypes)orderType
        };

        _transferProjectDbContext.Add(temporaryOrder);

        await _transferProjectDbContext.SaveChangesAsync();

        return Ok(orderId);
    }
}