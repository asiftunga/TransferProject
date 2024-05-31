using MiniApp1Api.Data.Entities;
using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.Services.Models;

public class IdentityUserModel
{
    public string UserId { get; set; }

    public string Email { get; set; }

    public UserTypes? Role { get; set; }

    public bool IsAdmin { get; set; }

    public UserApp User { get; set; }

    public bool HasRoleOfAdmin(UserTypes? role)
    {
        switch (role)
        {
            case null:
            case UserTypes.User:
            case UserTypes.Unknown:
                return false;
            case UserTypes.Admin:
                return true;
        }

        return false;
    }
}