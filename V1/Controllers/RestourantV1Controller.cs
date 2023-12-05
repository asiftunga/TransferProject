using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> PatchRestourants([FromForm] PatchRestourantWithFilesRequests request)
    {
        Claim? userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim.Value is null)
        {
            return Unauthorized();
        }

        Restourant? restourant = await _tmMealDbContext.Restourants
            .FirstOrDefaultAsync(x => x.UserId == userIdClaim.Value);

        if (restourant is null)
        {
            return BadRequest();
        }

        // Gelen verileri doğrudan Restourant nesnesine atayın
        restourant.MondayOpening = request.PatchData.MondayOpening?.ToTimeSpan();
        restourant.MondayClosing = request.PatchData.MondayClosing?.ToTimeSpan();
        restourant.TuesdayOpening = request.PatchData.TuesdayOpening?.ToTimeSpan();
        restourant.TuesdayClosing = request.PatchData.TuesdayClosing?.ToTimeSpan();
        restourant.WednesdayOpening = request.PatchData.WednesdayOpening?.ToTimeSpan();
        restourant.WednesdayClosing = request.PatchData.WednesdayClosing?.ToTimeSpan();
        restourant.ThursdayOpening = request.PatchData.ThursdayOpening?.ToTimeSpan();
        restourant.ThursdayClosing = request.PatchData.ThursdayClosing?.ToTimeSpan();
        restourant.FridayOpening = request.PatchData.FridayOpening?.ToTimeSpan();
        restourant.FridayClosing = request.PatchData.FridayClosing?.ToTimeSpan();
        restourant.SaturdayOpening = request.PatchData.SaturdayOpening?.ToTimeSpan();
        restourant.SaturdayClosing = request.PatchData.SaturdayClosing?.ToTimeSpan();
        restourant.SundayOpening = request.PatchData.SundayOpening?.ToTimeSpan();
        restourant.SundayClosing = request.PatchData.SundayClosing?.ToTimeSpan();
        restourant.ClosedDays = (int)request.PatchData.OpeningDaysBitMask;

        // Dosyaları işleme
        if (request.Files.Any())
        {
            string restaurantDirectory = Path.Combine("Restaurants", restourant.Id.ToString(), userIdClaim.Value);

            if (!Directory.Exists(restaurantDirectory))
            {
                Directory.CreateDirectory(restaurantDirectory);
            }

            foreach (var file in request.Files)
            {
                if (file.Length > 0)
                {
                    string filePath = Path.Combine(restaurantDirectory, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            // Dosya yolu veya klasör yolu kaydetme
            string pathToSave = request.Files.Count == 1
                ? Path.Combine(restaurantDirectory, request.Files.First().FileName)
                : restaurantDirectory;

             restourant.Url = pathToSave; // URL alanını güncelle
        }

        await _tmMealDbContext.SaveChangesAsync();
        return NoContent();
    }
}