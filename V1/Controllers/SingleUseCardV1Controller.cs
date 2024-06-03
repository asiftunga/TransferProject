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
    private readonly SendOrderInfoToAdminsEmailSenderBackgroundService _emailSenderService;
    private readonly TransferProjectDbContext _transferProjectDbContext;
    private readonly IIdentityServer _identityServer;

    public SingleUseCardV1Controller(
        UserManager<UserApp> userManager,
        SendOrderInfoToAdminsEmailSenderBackgroundService emailSenderService,
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

        bool singleCardOrder = await _transferProjectDbContext.SingleCardDetails.AsNoTracking().AnyAsync(x => x.OrderId == request.OrderId);

        if (singleCardOrder)
        {
            return BadRequest("There is already created order with this order id");
        }

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
            Currency = request.Currency,
            Payment = request.PaymentMethod,
            PaymentArea = request.PaymentArea,
            OrderTypes = request.OrderType,
            OrderStatus = OrderStatus.WaitingForMoneyTransfer,
            CreatedAt = now,
            CreatedBy = nameof(CreateSingleCardOrder),
            UpdatedAt = now,
            UpdatedBy = nameof(CreateSingleCardOrder)
        };

        _transferProjectDbContext.Add(order);

        SingleCardDetail singleCardDetail = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order.OrderId,
            UserId = userModel.UserId,
            Amount = order.Amount,
            OrderTypes = order.OrderTypes,
            Currency = order.Currency,
            Payment = order.Payment,
            OrderStatus = OrderStatus.WaitingForMoneyTransfer,
            CardName = null,
            CardNumber = null,
            CardDate = null,
            CVV = null,
            CreatedAt = now,
            CreatedBy = nameof(CreateSingleCardOrder),
            UpdatedAt = now,
            UpdatedBy = nameof(CreateSingleCardOrder)
        };

        _transferProjectDbContext.Add(singleCardDetail);

        await _transferProjectDbContext.SaveChangesAsync();

        _emailSenderService.QueueEmail(userModel.User.FirstName, userModel.User.Email!, request.OrderId, request.Amount, userModel.User.Id, order.Currency.ToString());

        CreateOrderResponse response = new()
        {
            Id = order.Id
        };

        return Created(new Uri(response.Id.ToString(), UriKind.Relative), response);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(SingleCardDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSingleCardOrder([FromRoute(Name = "orderId")] Guid orderId)
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        SingleCardDetail? singleCard = await _transferProjectDbContext.SingleCardDetails.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userModel.UserId);

        if (singleCard is null)
        {
            return NotFound();
        }

        return Ok(singleCard);
    }

    [HttpPatch("{orderId:guid}")] //only admins
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSingleCardOrder([FromRoute(Name = "orderId")] Guid orderId, [FromBody] UpdateSingleCardOrderRequest request)
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        if (!userModel.IsAdmin)
        {
            return Unauthorized();
        }

        SingleCardDetail? singleCard = await _transferProjectDbContext.SingleCardDetails.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == request.UserId);

        if (singleCard is null)
        {
            return NotFound();
        }

        if (singleCard.OrderStatus == OrderStatus.OrderCanceled)
        {
            return BadRequest("Single card with this status can not updated");
        }

        Order? order = await _transferProjectDbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == request.UserId);

        if (order is null)
        {
            return NotFound();
        }

        if (order.OrderStatus == OrderStatus.OrderCanceled)
        {
            return BadRequest("Single card with this status can not updated");
        }

        DateTime now = DateTime.UtcNow;

        order.OrderStatus = OrderStatus.OrderCompleted;
        order.UpdatedAt = now;
        order.UpdatedBy = nameof(UpdateSingleCardOrder);

        singleCard.CardName = request.CardName;
        singleCard.CardDate = request.CardDate;
        singleCard.CardNumber = request.CardNumber;
        singleCard.CVV = request.CVV;
        singleCard.UpdatedAt = now;
        singleCard.OrderStatus = OrderStatus.OrderCompleted;
        singleCard.UpdatedBy = nameof(UpdateSingleCardOrder);

        ApprovedOrder approvedOrder = new()
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            OrderId = orderId,
            IsRead = false
        };

        _transferProjectDbContext.Add(approvedOrder);

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
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        DateTime now = DateTime.UtcNow;

        Order? order = await _transferProjectDbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userModel.UserId && x.OrderStatus != OrderStatus.OrderCanceled);

        TemporaryOrder? temporaryOrder = await _transferProjectDbContext.TemporaryOrders.FirstOrDefaultAsync(x => x.OrderId == orderId);

        SingleCardDetail? singleCard = await _transferProjectDbContext.SingleCardDetails.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userModel.UserId);

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

        if (singleCard is not null)
        {
            singleCard.OrderStatus = OrderStatus.OrderCanceled;
            singleCard.UpdatedAt = now;
            singleCard.UpdatedBy = nameof(CancelSingleCardOrder);
        }

        await _transferProjectDbContext.SaveChangesAsync();

        return NoContent();
    }
}