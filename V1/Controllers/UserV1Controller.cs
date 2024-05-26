using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.Http;
using MiniApp1Api.BackgroundServices;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.V1.Models.Requests;
using MiniApp1Api.V1.Models.Responses;

namespace MiniApp1Api.V1.Controllers;


[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/[action]")]
public class UserV1Controller : ControllerBase
{
    private readonly UserManager<UserApp> _userManager;
    private readonly EmailSenderBackgroundService _emailSenderService;

    public UserV1Controller(
        UserManager<UserApp> userManager,
        EmailSenderBackgroundService emailSenderService)
    {
        _userManager = userManager;
        _emailSenderService = emailSenderService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

        if (user.PhoneNumber is not null)
        {
            bool existingUserExists = _userManager.Users.AsNoTracking().Any(x => x.PhoneNumber == request.PhoneNumber);

            if (existingUserExists)
            {
                return BadRequest("User phone already exists!");
            }
        }

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

    [HttpPost]
    [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        UserApp? user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Background service kullanarak kuyruğa e-posta bilgilerini ekle
        _emailSenderService.QueueEmail(user.FirstName, user.Email, token);

        return Ok("Password reset token sent.");
    }

    [HttpPost]
    [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        UserApp? user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

        if (result.Succeeded)
        {
            return Ok("Password has been reset.");
        }

        // Hata durumunda, hataları döndür
        return BadRequest(result.Errors);
    }

}