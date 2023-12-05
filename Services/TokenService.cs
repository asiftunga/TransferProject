using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniApp1Api.Configuration;
using MiniApp1Api.Data;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.Data.Enums;
using MiniApp1Api.Services.Models;

namespace MiniApp1Api.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<UserApp> _userManager;
    private readonly CustomTokenOption _customTokenOptions;
    private readonly TMMealDbContext _tmMealDbContext;

    public TokenService(
        UserManager<UserApp> userManager,
        IOptions<CustomTokenOption> customTokenOptions,
        TMMealDbContext tmMealDbContext)
    {
        _userManager = userManager;
        _customTokenOptions = customTokenOptions.Value;
        _tmMealDbContext = tmMealDbContext;
    }

    public async Task<TokenModel> CreateToken(UserApp userApp)
    {
        DateTime accessTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOptions.AccessTokenExpiration);
        DateTime refreshTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOptions.RefreshTokenExpiration);

        SecurityKey securityKey = SignService.GetSymmetricSecurtiyKey(_customTokenOptions.SecurityKey);

        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
            issuer: _customTokenOptions.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: await GetClaims(userApp, _customTokenOptions.Audience),
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        string token = handler.WriteToken(jwtSecurityToken);

        TokenModel tokenModel = new TokenModel()
        {
            AccessToken = token,
            RefreshToken = CreateRefreshToken(),
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration
        };

        return tokenModel;
    }

    public ClientTokenModel CreateTokenByClient(Client client)
    {
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOptions.AccessTokenExpiration);

        var securityKey = SignService.GetSymmetricSecurtiyKey(_customTokenOptions.SecurityKey);

        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
            issuer: _customTokenOptions.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: GetClaimsByClient(client),
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();

        var token = handler.WriteToken(jwtSecurityToken);

        var tokenDto = new ClientTokenModel
        {
            AccessToken = token,

            AccessTokenExpiration = accessTokenExpiration,
        };

        return tokenDto;
    }

    private string CreateRefreshToken()
    {
        byte[] numberByte = new Byte[32];

        using RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        rnd.GetBytes(numberByte);

        return Convert.ToBase64String(numberByte);
    }

    //auth olmasi gereken userlar icin
    private async Task<IEnumerable<Claim>> GetClaims(UserApp userApp, List<string> audiences)
    {
        var userList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userApp.Id),
            new Claim(JwtRegisteredClaimNames.Email, userApp.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

        // Kullanıcının rollerini al ve claim listesine ekle
        var userRoles = await _userManager.GetRolesAsync(userApp);
        userList.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (userRoles.Contains(UserTypes.RestourantOwner.ToString()))
        {
            // UserApp ile ilişkili restoranın durumunu sorgula
            RestourantStatus restaurantStatus = await _tmMealDbContext.Restourants
                .Where(r => r.UserId == userApp.Id)
                .Select(r => r.Status)
                .FirstOrDefaultAsync();

            // Eğer restoran bulunduysa ve durum bilgisi varsa, claim listesine ekle
            if (!string.IsNullOrEmpty(restaurantStatus.ToString()))
            {
                userList.Add(new Claim("restaurantStatus", restaurantStatus.ToString()));
            }
        }

        return userList;
    }

    //auth olmasi gerekmeyen userlarin erismesi icin
    private IEnumerable<Claim> GetClaimsByClient(Client client)
    {
        var claims = new List<Claim>();
        claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        new Claim(JwtRegisteredClaimNames.Sub, client.Id);

        return claims;
    }
}