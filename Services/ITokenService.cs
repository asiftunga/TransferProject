using TransferProject.Configuration;
using TransferProject.Data.Entities;
using TransferProject.Services.Models;

namespace TransferProject.Services;

public interface ITokenService
{
    Task<TokenModel> CreateToken(UserApp userApp);

    ClientTokenModel CreateTokenByClient(Client client);
}