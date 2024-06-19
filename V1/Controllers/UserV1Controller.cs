using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.Http;
using MiniApp1Api.BackgroundServices;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.Data.Enums;
using MiniApp1Api.Data.Identity;
using MiniApp1Api.V1.Models.Requests;
using MiniApp1Api.V1.Models.Responses;

namespace MiniApp1Api.V1.Controllers;


[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/[action]")]
public class UserV1Controller : ControllerBase
{
    private readonly CustomUserManager<UserApp> _userManager;
    private readonly ForgotPasswordEmailSenderBackgroundService _forgotPasswordEmailSenderService;

    public UserV1Controller(
        CustomUserManager<UserApp> userManager,
        ForgotPasswordEmailSenderBackgroundService forgotPasswordEmailSenderService)
    {
        _userManager = userManager;
        _forgotPasswordEmailSenderService = forgotPasswordEmailSenderService;
    }

    /// <summary>
    /// Kullanici olusturur.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
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

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, UserTypes.User.ToString());

        if (!roleResult.Succeeded)
        {
            ProblemDetails problemDetails = new()
            {
                Type = "user-creation-failed",
                Title = "User creation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User creation failed due to can not add user role to user"
            };

            return new ObjectResult(problemDetails);
        }

        CreateUserResponse response = new()
        {
            Id = user.Id
        };

        return Created(new Uri(response.Id, UriKind.Relative), response);
    }

    /// <summary>
    /// Sifre sifirlamak icin gelinen yer.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotUserPasswordRequest request)
    {
        UserApp? user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Background service kullanarak kuyruğa e-posta bilgilerini ekle
        _forgotPasswordEmailSenderService.QueueEmail(user.FirstName, user.Email, token);

        return Ok("Password reset token sent.");
    }

    /// <summary>
    /// Email linkinden sonra sifre sifirlamak icin sifre sifirlama tokeninin yollandigi yer.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
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