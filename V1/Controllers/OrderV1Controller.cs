using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.Http;
using TransferProject.BackgroundServices;
using TransferProject.Data;
using TransferProject.Data.Entities;
using TransferProject.Data.Enums;
using TransferProject.Data.Identity;
using TransferProject.Extensions;
using TransferProject.Models;
using TransferProject.Services;
using TransferProject.Services.Models;
using TransferProject.V1.Models.Requests;
using TransferProject.V1.Models.Responses;

namespace TransferProject.V1.Controllers;


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

    /// <summary>
    /// Kullanicinin herhangi bir bildirimi olup olmadigini header'da doner. Her dk basi buraya gelinmeli.
    /// </summary>
    /// <returns></returns>
    [HttpHead]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApprovedInfo()
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        bool anyUnreadMessage = await _transferProjectDbContext.ApprovedOrders.AsNoTracking().AnyAsync(x => x.UserId == userModel.UserId && !x.IsRead);

        HttpContext.Response.Headers.Add("X-IsAnyUnreadMessages", anyUnreadMessage.ToString());

        return NoContent();
    }

    /// <summary>
    /// Kullanicinin butun siparislerini doner. Okunma bilgisi dahil bir sekilde donus yapar.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(QueryOrderResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> QueryOrders([FromQuery] QueryOrderRequest request)
    {
        IdentityUserModel userModel = await _identityServer.GetAuthenticatedUser();

        IQueryable<Order> query = _transferProjectDbContext.Orders.AsNoTracking().Where(x => x.UserId == userModel.UserId);

        if (string.IsNullOrWhiteSpace(request.OrderBy))
        {
            request.OrderBy = nameof(Order.CreatedAt);
        }

        IPage<Order> pagedOrders = await query.ToPageAsync(request);

        List<QueryOrderResponse> responseList = new ();

        foreach (Order order in pagedOrders.Items)
        {
            bool isRead = await IsOrderRead(order.OrderId);

            responseList.Add(new QueryOrderResponse
            {
                Id = order.Id,
                OrderId = order.OrderId,
                UserId = order.UserId,
                Amount = order.Amount,
                Currency = order.Currency,
                OrderTypes = order.OrderTypes,
                CreatedAt = order.CreatedAt,
                CreatedBy = order.CreatedBy,
                UpdatedAt = order.UpdatedAt,
                UpdatedBy = order.UpdatedBy,
                OrderStatus = order.OrderStatus,
                IsRead = isRead
            });
        }

        Page<QueryOrderResponse> pagedResponse = new (responseList, pagedOrders.Index, pagedOrders.Size, pagedOrders.TotalCount);

        return Ok(pagedResponse);
    }

    /// <summary>
    /// order type id sine gore unique bir orderid olusturur.
    /// </summary>
    /// <param name="orderType"></param>
    /// <returns></returns>
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


    private async Task<bool> IsOrderRead(Guid orderId)
    {
        ApprovedOrder? isRead = await _transferProjectDbContext.ApprovedOrders.AsNoTracking().FirstOrDefaultAsync(m => m.OrderId == orderId);

        if (isRead is null || isRead.IsRead)
        {
            return false;
        }

        return true;
    }
}