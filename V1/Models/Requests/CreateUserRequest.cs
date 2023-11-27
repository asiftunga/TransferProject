namespace MiniApp1Api.V1.Models.Requests;

public class CreateUserRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PhoneNumber { get; set; }

    public string Password { get; set; }

    public string Email { get; set; }
}