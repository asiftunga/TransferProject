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
using MiniApp1Api.Services.Models;
using MiniApp1Api.V1.Models.Requests;

namespace MiniApp1Api.V1.Controllers;


[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/[action]")]
public class AuthV1Controller : ControllerBase
{
    private readonly UserManager<UserApp> _userManager;
    private readonly List<Client> _clients;
    private readonly ITokenService _tokenService;
    private readonly TransferProjectDbContext _transferProjectDbContext;

    public AuthV1Controller(
        IOptions<List<Client>> optionsClient,
        UserManager<UserApp> userManager,
        ITokenService tokenService,
        TransferProjectDbContext transferProjectDbContext)
    {
        _clients = optionsClient.Value;
        _transferProjectDbContext = transferProjectDbContext;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Login olmayi saglar.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [HttpPost]
    [ProducesResponseType(typeof(TokenModel),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUserToken([FromBody] CreateTokenRequest request) //login
    {
        if (request is null)
        {
            throw new ArgumentNullException();
        }

        UserApp? user = await _transferProjectDbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email && !x.IsDeleted);

        if (user is null)
        {
            ProblemDetails problemDetails = new()
            {
                Type = "email-or-password-is-wrong",
                Title = "Email Or Passwaord Is Wrong",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The specified email or password could not provided."
            };

            return new ObjectResult(problemDetails);
        }

        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            if (await _userManager.IsLockedOutAsync(user))
            {
                ProblemDetails problemDetail = new()
                {
                    Type = "Your-account-has-been-locked.",
                    Title = "Your Account Has Been Locked.",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "Your account has been locked."
                };

                return new ObjectResult(problemDetail);
            }

            await _userManager.AccessFailedAsync(user);

            ProblemDetails problemDetails = new()
            {
                Type = "email-or-password-is-wrong",
                Title = "Email Or Passwaord Is Wrong",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The specified email or password could not provided."
            };

            return new ObjectResult(problemDetails);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {

            ProblemDetails problemDetails = new()
            {
                Type = "Your-account-has-been-locked.",
                Title = "Your Account Has Been Locked.",
                Status = StatusCodes.Status429TooManyRequests,
                Detail = "Your account has been locked."
            };

            return new ObjectResult(problemDetails);
        }

        TokenModel token = await _tokenService.CreateToken(user);

        UserRefreshToken? userRefreshToken = await _transferProjectDbContext.UserRefreshTokens
            .Where(x => x.UserId == user.Id && !user.IsDeleted).SingleOrDefaultAsync();

        if (userRefreshToken is null)
        {
            await _transferProjectDbContext.AddAsync(new UserRefreshToken
            {
                UserId = user.Id,
                Code = token.RefreshToken,
                Expiration = token.RefreshTokenExpiration
            });
        }
        else
        {
            userRefreshToken.Code = token.RefreshToken;
            userRefreshToken.Expiration = token.RefreshTokenExpiration;
        }

        await _transferProjectDbContext.SaveChangesAsync();

        return Ok(token);
    }

    #region deprecatedRegion

    // [HttpPost("")]
    // public async Task<IActionResult> CreateRestourantOwnerToken([FromBody] CreateTokenRequest forAdminsRequest)
    // {
    //     if (forAdminsRequest is null)
    //     {
    //         throw new ArgumentNullException();
    //     }
    //
    //     UserApp? user = await _userManager.FindByEmailAsync(forAdminsRequest.Email);
    //
    //     if (user is null)
    //     {
    //         ProblemDetails problemDetails = new()
    //         {
    //             Type = "email-or-password-is-wrong",
    //             Title = "Email Or Passwaord Is Wrong",
    //             Status = StatusCodes.Status400BadRequest,
    //             Detail = "The specified email or password could not provided."
    //         };
    //
    //         return new ObjectResult(problemDetails);
    //     }
    //
    //     bool isPasswordValid = await _userManager.CheckPasswordAsync(user, forAdminsRequest.Password);
    //
    //     if (!isPasswordValid)
    //     {
    //         if (await _userManager.IsLockedOutAsync(user))
    //         {
    //             ProblemDetails problemDetail = new()
    //             {
    //                 Type = "Your-account-has-been-locked.",
    //                 Title = "Your Account Has Been Locked.",
    //                 Status = StatusCodes.Status429TooManyRequests,
    //                 Detail = "Your account has been locked."
    //             };
    //
    //             return new ObjectResult(problemDetail);
    //         }
    //
    //         await _userManager.AccessFailedAsync(user);
    //
    //         ProblemDetails problemDetails = new()
    //         {
    //             Type = "email-or-password-is-wrong",
    //             Title = "Email Or Passwaord Is Wrong",
    //             Status = StatusCodes.Status400BadRequest,
    //             Detail = "The specified email or password could not provided."
    //         };
    //
    //         return new ObjectResult(problemDetails);
    //     }
    //
    //     if (await _userManager.IsLockedOutAsync(user))
    //     {
    //
    //         ProblemDetails problemDetails = new()
    //         {
    //             Type = "Your-account-has-been-locked.",
    //             Title = "Your Account Has Been Locked.",
    //             Status = StatusCodes.Status429TooManyRequests,
    //             Detail = "Your account has been locked."
    //         };
    //
    //         return new ObjectResult(problemDetails);
    //     }
    //
    //     bool isRoleValid = await _userManager.IsInRoleAsync(user, UserTypes.RestourantOwner.ToString());
    //
    //     if (!isRoleValid)
    //     {
    //         ProblemDetails problemDetails = new()
    //         {
    //             Type = "unauthorized",
    //             Title = "Unauthorized",
    //             Status = StatusCodes.Status401Unauthorized,
    //             Detail = "Unauthorized"
    //         };
    //
    //         return new ObjectResult(problemDetails);
    //     }
    //
    //     TokenModel token = await _tokenService.CreateToken(user);
    //
    //     UserRefreshToken? userRefreshToken = await _transferProjectDbContext.UserRefreshTokens
    //         .Where(x => x.UserId == user.Id && !user.IsDeleted).SingleOrDefaultAsync();
    //
    //     if (userRefreshToken is null)
    //     {
    //         await _transferProjectDbContext.AddAsync(new UserRefreshToken
    //         {
    //             UserId = user.Id,
    //             Code = token.RefreshToken,
    //             Expiration = token.RefreshTokenExpiration
    //         });
    //     }
    //     else
    //     {
    //         userRefreshToken.Code = token.RefreshToken;
    //         userRefreshToken.Expiration = token.RefreshTokenExpiration;
    //     }
    //
    //     await _transferProjectDbContext.SaveChangesAsync();
    //
    //     return Ok(token);
    // }

    // [HttpPost]
    // public async Task<IActionResult> CreateTokenByClient([FromBody] CreateTokenByClientRequest forAdminsRequest)
    // {
    //     Client? client = _clients.SingleOrDefault(x => x.Id == forAdminsRequest.ClientId && x.Secret == forAdminsRequest.ClientSecret);
    //
    //     if (client is null)
    //     {
    //         ProblemDetails problemDetails = new()
    //         {
    //             Type = "clientid-or-clientsecret-not-found",
    //             Title = "ClientId Or ClientSecret Not Found",
    //             Status = StatusCodes.Status404NotFound,
    //             Detail = "The specified clientId or clientSecret could not provided."
    //         };
    //
    //         return new ObjectResult(problemDetails);
    //     }
    //
    //     ClientTokenModel token = _tokenService.CreateTokenByClient(client);
    //
    //     return Ok(token);
    // }

    //todo : deprecated, return this method later

    #endregion

    /// <summary>
    /// Access token suresi bittiginde cikis yapmadan refresh token ile yeni bir token almayi saglar
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(TokenModel),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTokenByRefreshToken(string refreshToken)
    {
        UserRefreshToken? refreshTokenExists = await _transferProjectDbContext.UserRefreshTokens
            .Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

        if (refreshTokenExists is null)
        {
            ProblemDetails problemDetails = new()
            {
                Type = "refresh-token-not-found",
                Title = "Refresh Token Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = "The provided refresh token not found."
            };

            return new ObjectResult(problemDetails);
        }

        UserApp? user = _transferProjectDbContext.Users.AsNoTracking().FirstOrDefault(x => x.Id == refreshTokenExists.UserId && !x.IsDeleted);

        if (user is null)
        {
            ProblemDetails problemDetails = new()
            {
                Type = "user-id-not-found",
                Title = "User Id Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = "The provided userid not found."
            };

            return new ObjectResult(problemDetails);
        }

        TokenModel token = await _tokenService.CreateToken(user);

        refreshTokenExists.Code = token.RefreshToken;
        refreshTokenExists.Expiration = token.RefreshTokenExpiration;

        await _transferProjectDbContext.SaveChangesAsync();

        return Ok(token);
    }

    /// <summary>
    /// refresh tokeni silmeyi saglar. Logout yapildiginda gelinir
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeRefreshToken(string refreshToken) //sign out
    {
        UserRefreshToken? existingRefreshToken =
            await _transferProjectDbContext.UserRefreshTokens.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

        if (existingRefreshToken is null)
        {
            ProblemDetails problemDetails = new()
            {
                Type = "refresh-token-not-found",
                Title = "Refresh Token Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = "The provided refresh token not found."
            };

            return new ObjectResult(problemDetails);
        }

        _transferProjectDbContext.Remove(existingRefreshToken);

        await _transferProjectDbContext.SaveChangesAsync();

        return NoContent();
    }
}