namespace TransferProject.V1.Models.Requests;

public class TimeModel
{
    public int Hours { get; set; }
    public int Minutes { get; set; }

    public TimeSpan ToTimeSpan()
    {
        return new TimeSpan(Hours, Minutes, 0);
    }
}