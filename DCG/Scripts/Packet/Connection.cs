using System;

[Serializable]
public class Connection
{
    public DateTime time;

    public int CheckPing()
    {
        DateTime _now = DateTime.UtcNow;
        TimeSpan _ts = _now - time;

        return (int)Math.Round(_ts.TotalMilliseconds);
    }
}