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

    public SingleUseCardV1Controller(
        UserManager<UserApp> userManager,
        EmailSenderBackgroundService emailSenderService,
        TransferProjectDbContext transferProjectDbContext)
    {
        _userManager = userManager;
        _emailSenderService = emailSenderService;
        _transferProjectDbContext = transferProjectDbContext;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateSingleUsedCardOrderRequest request)
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

        TemporaryOrder? temporaryOrder = await _transferProjectDbContext.TemporaryOrders.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == request.OrderId);

        if (temporaryOrder is null || temporaryOrder.OrderType != request.OrderType)
        {
            return Unauthorized();
        }

        Order order = new()
        {
            OrderId = request.OrderId,
            UserId = user.Id,
            Amount = request.Amount,
            OrderTypes = request.OrderType,
            CreatedAt = now,
            CreatedBy = nameof(CreateOrder),
            UpdatedAt = now,
            UpdatedBy = nameof(CreateOrder),
            OrderStatus = OrderStatus.WaitingForMoneyTransfer
        };

        _transferProjectDbContext.Add(order);

        _emailSenderService.QueueEmail(user.FirstName, user.Email!, request.OrderId, request.Amount, user.Id);

        await _transferProjectDbContext.SaveChangesAsync();

        CreateOrderResponse response = new()
        {
            Id = order.Id
        };

        return Created(new Uri(response.Id.ToString(), UriKind.Relative), response);
    }
}