using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Web.Http;
using MiniApp1Api.BackgroundServices;
using MiniApp1Api.Configuration;
using MiniApp1Api.Data;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.Data.Enums;
using MiniApp1Api.Services;
using MiniApp1Api.V1.Models.Requests;
using MiniApp1Api.V1.Models.Responses;

namespace MiniApp1Api.V1.Controllers;


[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/users")]
public class UserV1Controller : ControllerBase
{
    private readonly UserManager<UserApp> _userManager;
    private readonly List<Client> _clients;
    private readonly ITokenService _tokenService;
    private readonly TMMealDbContext _tmMealDbContext;
    private readonly EmailSenderBackgroundService _emailSenderService;

    public UserV1Controller(
        IOptions<List<Client>> optionsClient,
        UserManager<UserApp> userManager,
        ITokenService tokenService,
        TMMealDbContext tmMealDbContext,
        EmailSenderBackgroundService emailSenderService)
    {
        _clients = optionsClient.Value;
        _tmMealDbContext = tmMealDbContext;
        _userManager = userManager;
        _tokenService = tokenService;
        _emailSenderService = emailSenderService;
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = new UserApp
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = (request.FirstName+request.Email).Replace(" ",""),
            UserTypes = UserTypes.User,
            LockoutEnabled = true
        };

        user.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            string combinedErrors = string.Join(", ", errors);

            ProblemDetails problemDetails = new ProblemDetails
            {
                Type = "user-creation-failed",
                Title = "User creation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User creation failed due to the following errors: " + combinedErrors
            };

            return new ObjectResult(problemDetails);
        }

        await _userManager.AddToRoleAsync(user, UserTypes.User.ToString());

        await _tmMealDbContext.SaveChangesAsync();

        CreateUserResponse response = new CreateUserResponse()
        {
            Id = user.Id
        };

        return Created(new Uri(response.Id, UriKind.Relative), response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Background service kullanarak kuyruğa e-posta bilgilerini ekle
        _emailSenderService.QueueEmail(user.Email, token);

        return Ok("Password reset token sent.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

        if (result.Succeeded)
        {
            return Ok("Password has been reset.");
        }

        // Hata durumunda, hataları döndür
        return BadRequest(result.Errors);
    }

}