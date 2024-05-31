using MiniApp1Api.Services.Models;

namespace MiniApp1Api.Services;

public interface IIdentityServer
{
    public ValueTask<IdentityUserModel> GetAuthenticatedUser();
}