using TransferProject.Services.Models;

namespace TransferProject.Services;

public interface IIdentityServer
{
    public ValueTask<IdentityUserModel> GetAuthenticatedUser();
}