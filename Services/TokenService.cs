using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TransferProject.Configuration;
using TransferProject.Data;
using TransferProject.Data.Entities;
using TransferProject.Services.Models;

namespace TransferProject.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<UserApp> _userManager;
    private readonly CustomTokenOption _customTokenOptions;
    private readonly TransferProjectDbContext _transferProjectDbContext;

    public TokenService(
        UserManager<UserApp> userManager,
        IOptions<CustomTokenOption> customTokenOptions,
        TransferProjectDbContext transferProjectDbContext)
    {
        _userManager = userManager;
        _customTokenOptions = customTokenOptions.Value;
        _transferProjectDbContext = transferProjectDbContext;
    }

    public async Task<TokenModel> CreateToken(UserApp userApp)
    {
        DateTime accessTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOptions.AccessTokenExpiration);
        DateTime refreshTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOptions.RefreshTokenExpiration);

        SecurityKey securityKey = SignService.GetSymmetricSecurtiyKey(_customTokenOptions.SecurityKey);

        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new(
            issuer: _customTokenOptions.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: await GetClaims(userApp, _customTokenOptions.Audience),
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler handler = new();

        string token = handler.WriteToken(jwtSecurityToken);

        TokenModel tokenModel = new()
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
        DateTime accessTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOptions.AccessTokenExpiration);

        SecurityKey? securityKey = SignService.GetSymmetricSecurtiyKey(_customTokenOptions.SecurityKey);

        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new(
            issuer: _customTokenOptions.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: GetClaimsByClient(client),
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler? handler = new();

        string? token = handler.WriteToken(jwtSecurityToken);

        ClientTokenModel? tokenDto = new()
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
        List<Claim>? userList = new()
        {
            new Claim(ClaimTypes.NameIdentifier, userApp.Id),
            new Claim(JwtRegisteredClaimNames.Email, userApp.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

        // Kullanıcının rollerini al ve claim listesine ekle
        IList<string>? userRoles = await _userManager.GetRolesAsync(userApp);
        userList.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        return userList;
    }

    //auth olmasi gerekmeyen userlarin erismesi icin
    private IEnumerable<Claim> GetClaimsByClient(Client client)
    {
        List<Claim>? claims = new();
        claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        new Claim(JwtRegisteredClaimNames.Sub, client.Id);

        return claims;
    }
}