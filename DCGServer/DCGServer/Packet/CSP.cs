public class CSP : Packet
{
    public DateTime time;

    public override void Run(Client client)
    {
        if(client.gameId <= 0)
        {
            Server.clients.Remove(client);
            Server.Queue(client.id, client);
        }
    }

    public int Run()
    {
        DateTime _now = DateTime.UtcNow;
        TimeSpan _ts = _now - time;

        return (int)Math.Round(_ts.TotalMilliseconds);
    }
}