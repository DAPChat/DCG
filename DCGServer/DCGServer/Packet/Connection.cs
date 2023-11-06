

public class Connection
{
    public DateTime time;

    public int CheckPing()
    {
        DateTime _now = DateTime.Now;
        TimeSpan _ts = new TimeSpan(time.Ticks - _now.Ticks);

        return (int)Math.Round(_ts.TotalMilliseconds);
    }
}