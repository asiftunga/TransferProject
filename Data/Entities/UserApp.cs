using Microsoft.AspNetCore.Identity;

namespace TransferProject.Data.Entities;

public class UserApp : IdentityUser
{

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string? Address { get; set; }

    public string IpAddress { get; set; }

    public bool IsDeleted { get; set; }
}