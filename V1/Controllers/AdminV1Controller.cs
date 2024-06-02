using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.Http;
using MiniApp1Api.BackgroundServices;
using MiniApp1Api.Data;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.Data.Enums;
using MiniApp1Api.Filters;
using MiniApp1Api.Services;
using MiniApp1Api.Services.Models;
using MiniApp1Api.V1.Models.Requests;
using MiniApp1Api.V1.Models.Responses;
using TransferProject.Extensions;
using TransferProject.Models;
using TransferProject.ObjectResults;

namespace MiniApp1Api.V1.Controllers;


[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/[controller]/[action]")]
public class AdminV1Controller : ControllerBase
{
    private readonly UserManager<UserApp> _userManager;
    private readonly EmailSenderBackgroundService _emailSenderService;
    private readonly TransferProjectDbContext _transferProjectDbContext;
    private readonly IIdentityServer _identityServer;

    public AdminV1Controller(
        UserManager<UserApp> userManager,
        EmailSenderBackgroundService emailSenderService,
        TransferProjectDbContext transferProjectDbContext, IIdentityServer identityServer)
    {
        _userManager = userManager;
        _emailSenderService = emailSenderService;
        _transferProjectDbContext = transferProjectDbContext;
        _identityServer = identityServer;
    }

    [HttpGet]
    [ProducesResponseType(typeof(QueryOrderResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> QueryOrders([FromQuery] QueryOrderRequest request)
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        if (!userModel.IsAdmin)
        {
            return Unauthorized();
        }

        IQueryable<Order> query = _transferProjectDbContext.Orders.AsNoTracking();

        if (request.AmountStart.HasValue)
        {
            query = query.Where(x => x.Amount > request.AmountStart);
        }

        if (request.AmountEnd.HasValue)
        {
            query = query.Where(x => x.Amount < request.AmountEnd);
        }

        if (request.OrderStatus.HasValue)
        {
            query = query.Where(x => x.OrderStatus == request.OrderStatus);
        }

        if (request.OrderType.HasValue)
        {
            query = query.Where(x => x.OrderTypes == request.OrderType);
        }

        if (request.Currency.HasValue)
        {
            query = query.Where(x => x.Currency == request.Currency);
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(x => x.UserId == request.UserId);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date > request.StartDate.Value.Date);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date < request.EndDate.Value.Date);
        }

        if (string.IsNullOrWhiteSpace(request.OrderBy))
        {
            request.OrderBy = nameof(Order.CreatedAt);
        }

        IPage<QueryOrderResponse> response = await query.Select(x => new QueryOrderResponse
        {
            Id = x.Id,
            OrderId = x.OrderId,
            UserId = x.UserId,
            Amount = x.Amount,
            Currency = x.Currency,
            OrderTypes = x.OrderTypes,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            OrderStatus = x.OrderStatus
        }).ToPageAsync(request);

        return new PageResult<QueryOrderResponse>(response);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(GetOrderInfoResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OrderInfo([FromRoute(Name = "orderId")] Guid orderId)
    {
        return Ok();
    }

    [HttpPatch("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus([FromRoute(Name = "orderId")] Guid orderId, [FromBody] PatchOrderRequest patchOrderRequest)
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        if (!userModel.IsAdmin)
        {
            return Unauthorized();
        }

        DateTime now = DateTime.UtcNow;

        Order? order = await _transferProjectDbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == patchOrderRequest.UserId.ToString());

        TemporaryOrder? temporaryOrder = await _transferProjectDbContext.TemporaryOrders.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == patchOrderRequest.UserId);

        if (order is null || temporaryOrder is null)
        {
            ProblemDetails problemDetails = new()
            {
                Type = "order-not-found!",
                Title = "Order Not Found!",
                Status = StatusCodes.Status404NotFound,
                Detail = "Order not found!"
            };

            throw new ProblemDetailsException(problemDetails);
        }


        if (order.OrderTypes == OrderTypes.SingleUseCard)
        {
            SingleCardDetail? singleCardDetail = await _transferProjectDbContext.SingleCardDetails.FirstOrDefaultAsync(x => x.UserId == patchOrderRequest.UserId.ToString() && x.OrderId == orderId);

            if (singleCardDetail is not null)
            {
                singleCardDetail.OrderStatus = patchOrderRequest.Status;
            }
        }

        order.OrderStatus = patchOrderRequest.Status;
        order.UpdatedAt = now;
        order.UpdatedBy = userModel.Email;

        if (patchOrderRequest.Status == OrderStatus.OrderCanceled)
        {
            _transferProjectDbContext.Remove(temporaryOrder);
        }

        await _transferProjectDbContext.SaveChangesAsync();

        return Ok();
    }
}