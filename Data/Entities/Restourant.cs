using Microsoft.AspNetCore.Identity;
using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.Data.Entities;

public class Restourant
{
    public Guid Id { get; set; }
    public string UserId { get; set; }

    public string City { get; set; }            // İl

    public string District { get; set; }        // İlçe

    public string Neighborhood { get; set; }    // Mahalle

    public bool DeliveryService { get; set; }   // Paket Servis hizmeti

    public string PassportOrTaxNumber { get; set; } // Pasaport Numarası / Vergi Numarası

    public string RestaurantName { get; set; }  // Restoran Adı

    public string CuisineType { get; set; }     // Mutfak Türü

    public string ReferenceCode { get; set; }   // Referans Kodu

    public string? RestourantPhoneNumber { get; set; }     // telefon numarası

    public bool IsDeleted { get; set; }

    public RestourantStatus Status { get; set; }

    public TimeSpan? MondayOpening { get; set; }

    public TimeSpan? MondayClosing { get; set; }

    public TimeSpan? TuesdayOpening { get; set; }

    public TimeSpan? TuesdayClosing { get; set; }

    public TimeSpan? WednesdayOpening { get; set; }

    public TimeSpan? WednesdayClosing { get; set; }

    public TimeSpan? ThursdayOpening { get; set; }

    public TimeSpan? ThursdayClosing { get; set; }

    public TimeSpan? FridayOpening { get; set; }

    public TimeSpan? FridayClosing { get; set; }

    public TimeSpan? SaturdayOpening { get; set; }

    public TimeSpan? SaturdayClosing { get; set; }

    public TimeSpan? SundayOpening { get; set; }

    public TimeSpan? SundayClosing { get; set; }

    public int ClosedDays { get; set; }

    public string? Url { get; set; }
}