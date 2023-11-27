using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Web.Http;
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
[Route("api/[controller]/restourants")]
public class RestourantV1Controller : ControllerBase
{
    private readonly UserManager<UserApp> _userManager;
    private readonly List<Client> _clients;
    private readonly ITokenService _tokenService;
    private readonly TMMealDbContext _tmMealDbContext;

    public RestourantV1Controller(
        IOptions<List<Client>> optionsClient,
        UserManager<UserApp> userManager,
        ITokenService tokenService,
        TMMealDbContext tmMealDbContext)
    {
        _clients = optionsClient.Value;
        _tmMealDbContext = tmMealDbContext;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateRestourantAndOwner([FromBody] CreateRestourantAndOwnerRequest request)
    {
        var user = new UserApp
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = (request.FirstName+request.Email).Replace(" ",""),
            UserTypes = UserTypes.User
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

        Restourant restourant = new Restourant
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            City = request.City,
            District = request.District,
            Neighborhood = request.Neighborhood,
            DeliveryService = request.DeliveryService,
            PassportOrTaxNumber = request.PassportOrTaxNumber,
            RestaurantName = request.RestaurantName,
            CuisineType = request.CuisineType,
            ReferenceCode = request.ReferenceCode,
            RestourantPhoneNumber = request.RestourantPhoneNumber,
            IsDeleted = false,
            Status = RestourantStatus.Waiting,
        };

        _tmMealDbContext.Add(restourant);

        await _userManager.AddToRoleAsync(user, UserTypes.RestourantOwner.ToString());

        await _tmMealDbContext.SaveChangesAsync();

        CreateRestourantAndOwnerResponse response = new CreateRestourantAndOwnerResponse()
        {
            Id = user.Id,
            RestourantId = restourant.Id
        };

        return Created(new Uri(response.Id, UriKind.Relative), response);
    }
}