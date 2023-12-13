using System;

namespace packets
{
    public class CSP : Packet
    {
        public DateTime time;

        public int Run()
        {
            DateTime _now = DateTime.UtcNow;
            TimeSpan _ts = _now - time;

            return (int)Math.Round(_ts.TotalMilliseconds);
        }
    }
}