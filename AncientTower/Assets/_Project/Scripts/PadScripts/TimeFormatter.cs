using System;

public static class TimeFormatter
{
    public static string FormatMessageTime(float timeStamp)
    {
        DateTime msgTime = new DateTime(1970, 1, 1).AddSeconds(timeStamp);
        DateTime now = DateTime.Now;
        
        if (msgTime.Date == now.Date)
            return msgTime.ToString("HH:mm");
        else if (msgTime.Date == now.Date.AddDays(-1))
            return $"昨天 {msgTime:HH:mm}";
        else if (msgTime.Year == now.Year)
            return msgTime.ToString("MM-dd HH:mm");
        else
            return msgTime.ToString("yyyy-MM-dd HH:mm");
    }
}