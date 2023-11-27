using MiniApp1Api.Configuration;
using MiniApp1Api.Data.Entities;
using MiniApp1Api.Services.Models;

namespace MiniApp1Api.Services;

public interface ITokenService
{
    Task<TokenModel> CreateToken(UserApp userApp);

    ClientTokenModel CreateTokenByClient(Client client);
}