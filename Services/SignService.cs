using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MiniApp1Api.Services;

public static class SignService
{
    public static SecurityKey GetSymmetricSecurtiyKey(string securtiyKey)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securtiyKey));
    }
}