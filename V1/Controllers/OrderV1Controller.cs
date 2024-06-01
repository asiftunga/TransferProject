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
public class OrderV1Controller : ControllerBase
{
    private readonly UserManager<UserApp> _userManager;
    private readonly EmailSenderBackgroundService _emailSenderService;
    private readonly TransferProjectDbContext _transferProjectDbContext;

    public OrderV1Controller(
        UserManager<UserApp> userManager,
        EmailSenderBackgroundService emailSenderService,
        TransferProjectDbContext transferProjectDbContext)
    {
        _userManager = userManager;
        _emailSenderService = emailSenderService;
        _transferProjectDbContext = transferProjectDbContext;
    }

    [HttpHead]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApprovedInfo()
    {
        Claim? userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim?.Value is null)
        {
            return Unauthorized();
        }

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

        Guid orderId = Guid.NewGuid();

        TemporaryOrder temporaryOrder = new()
        {
            UserId = Guid.Parse(userIdClaim.Value),
            OrderId = orderId,
            OrderType = (OrderTypes)orderType
        };

        _transferProjectDbContext.Add(temporaryOrder);

        await _transferProjectDbContext.SaveChangesAsync();

        return Ok(orderId);
    }
}