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
[ApiVersion("1.0")]
[Route("api/[controller]/orders")]
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

    [Authorize]
    [HttpGet("order-info/{email}/{orderType:int}")]
    public async Task<IActionResult> GetOrderInfo([FromRoute(Name = "email")] string email, [FromRoute(Name = "orderType")] int orderType)
    {
        UserApp? user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        Claim? userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim.Value is null)
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

    [HttpPost("")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        UserApp user = new()
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = (request.FirstName+request.Email).Replace(" ",""),
            LockoutEnabled = true,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        IdentityResult result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            List<string> errors = result.Errors.Select(e => e.Description).ToList();
            string combinedErrors = string.Join(", ", errors);

            ProblemDetails problemDetails = new()
            {
                Type = "user-creation-failed",
                Title = "User creation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User creation failed due to the following errors: " + combinedErrors
            };

            return new ObjectResult(problemDetails);
        }

        CreateUserResponse response = new()
        {
            Id = user.Id
        };

        return Created(new Uri(response.Id, UriKind.Relative), response);
    }

}