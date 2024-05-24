using Microsoft.AspNetCore.Identity;
using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.Data.Entities;

public class UserApp : IdentityUser
{

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string? Address { get; set; }

    public string IpAddress { get; set; }

    public bool IsDeleted { get; set; }
}