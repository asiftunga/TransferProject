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
using MiniApp1Api.Services;
using MiniApp1Api.Services.Models;
using MiniApp1Api.V1.Models.Requests;
using MiniApp1Api.V1.Models.Responses;

namespace MiniApp1Api.V1.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/[controller]/[action]")]
public class SingleUseCardV1Controller : ControllerBase
{
    private readonly UserManager<UserApp> _userManager;
    private readonly EmailSenderBackgroundService _emailSenderService;
    private readonly TransferProjectDbContext _transferProjectDbContext;
    private readonly IIdentityServer _identityServer;

    public SingleUseCardV1Controller(
        UserManager<UserApp> userManager,
        EmailSenderBackgroundService emailSenderService,
        TransferProjectDbContext transferProjectDbContext, IIdentityServer identityServer)
    {
        _userManager = userManager;
        _emailSenderService = emailSenderService;
        _transferProjectDbContext = transferProjectDbContext;
        _identityServer = identityServer;
    }
//todo bu orderid ye sahip baska bir kayit varsa unauth don her yerde ama ozellikle order kisimlarinda

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSingleCardOrder([FromBody] CreateSingleUsedCardOrderRequest request)
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        DateTime now = DateTime.UtcNow;

        TemporaryOrder? temporaryOrder = await _transferProjectDbContext.TemporaryOrders.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == request.OrderId);

        if (temporaryOrder is null || temporaryOrder.OrderType != request.OrderType)
        {
            return Unauthorized();
        }

        Order order = new()
        {
            OrderId = request.OrderId,
            UserId = userModel.User.Id,
            Amount = request.Amount,
            OrderTypes = request.OrderType,
            OrderStatus = OrderStatus.WaitingForMoneyTransfer,
            CreatedAt = now,
            CreatedBy = nameof(CreateSingleCardOrder),
            UpdatedAt = now,
            UpdatedBy = nameof(CreateSingleCardOrder)
        };

        _transferProjectDbContext.Add(order);

        _emailSenderService.QueueEmail(userModel.User.FirstName, userModel.User.Email!, request.OrderId, request.Amount, userModel.User.Id);

        await _transferProjectDbContext.SaveChangesAsync();

        CreateOrderResponse response = new()
        {
            Id = order.Id
        };

        return Created(new Uri(response.Id.ToString(), UriKind.Relative), response);
    }

    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSingleCardOrder([FromRoute(Name = "orderId")] Guid orderId)
    {
        await _transferProjectDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelSingleCardOrder([FromRoute(Name = "orderId")] Guid orderId)
    {
        Claim? email = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

        if (email?.Value is null)
        {
            return Unauthorized();
        }

        UserApp? user = await _userManager.FindByEmailAsync(email.Value);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        Claim? userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim?.Value is null || user.Id != userIdClaim.Value)
        {
            return Unauthorized();
        }

        DateTime now = DateTime.UtcNow;

        Order? order = await _transferProjectDbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId && x.OrderStatus != OrderStatus.OrderCanceled);

        TemporaryOrder? temporaryOrder = await _transferProjectDbContext.TemporaryOrders.FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order is not null)
        {
            order.OrderStatus = OrderStatus.OrderCanceled;
            order.UpdatedAt = now;
            order.UpdatedBy = nameof(CancelSingleCardOrder);
        }

        if (temporaryOrder is not null)
        {
            _transferProjectDbContext.Remove(temporaryOrder);
        }

        await _transferProjectDbContext.SaveChangesAsync();

        return NoContent();
    }
}