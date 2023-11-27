namespace MiniApp1Api.V1.Models.Requests;

public class CreateRestourantAndOwnerRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PhoneNumber { get; set; }

    public string Password { get; set; }

    public string Email { get; set; }

    public string City { get; set; }            // İl

    public string District { get; set; }        // İlçe

    public string Neighborhood { get; set; }    // Mahalle

    public bool DeliveryService { get; set; }   // Paket Servis hizmeti

    public string PassportOrTaxNumber { get; set; } // Pasaport Numarası / Vergi Numarası

    public string RestaurantName { get; set; }  // Restoran Adı

    public string CuisineType { get; set; }     // Mutfak Türü

    public string ReferenceCode { get; set; }   // Referans Kodu

    public string? RestourantPhoneNumber { get; set; }     // telefon numarası
}