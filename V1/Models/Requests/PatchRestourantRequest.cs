namespace MiniApp1Api.V1.Models.Requests;

public class PatchRestourantRequest
{
    public TimeModel? MondayOpening { get; set; }
    public TimeModel? MondayClosing { get; set; }

    public TimeModel? TuesdayOpening { get; set; }
    public TimeModel? TuesdayClosing { get; set; }

    public TimeModel? WednesdayOpening { get; set; }
    public TimeModel? WednesdayClosing { get; set; }

    public TimeModel? ThursdayOpening { get; set; }
    public TimeModel? ThursdayClosing { get; set; }

    public TimeModel? FridayOpening { get; set; }
    public TimeModel? FridayClosing { get; set; }

    public TimeModel? SaturdayOpening { get; set; }
    public TimeModel? SaturdayClosing { get; set; }

    public TimeModel? SundayOpening { get; set; }
    public TimeModel? SundayClosing { get; set; }

    public byte OpeningDaysBitMask { get; set; }
}