namespace CruiseShip.UpdateAgent.Models;

public class MaintenanceWindow
{
    public List<string> Days { get; set; } = new();
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TimeZone { get; set; } = "UTC";
    
    public bool IsInWindow(DateTime currentTime)
    {
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
        var localTime = TimeZoneInfo.ConvertTime(currentTime, timeZoneInfo);
        
        var currentDay = localTime.DayOfWeek.ToString();
        var currentTimeOfDay = localTime.TimeOfDay;
        
        return Days.Contains(currentDay) && 
               currentTimeOfDay >= StartTime && 
               currentTimeOfDay <= EndTime;
    }
}
