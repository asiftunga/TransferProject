namespace TransferProject.V1.Models.Requests;

public class CreateUserRequest
{
    public string Email { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PhoneNumber { get; set; }

    public string Password { get; set; }
}