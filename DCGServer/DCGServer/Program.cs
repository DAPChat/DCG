using System.Net;
using System.Net.Sockets;

class Program
{
    public static void Main()
	{
        Server.Start();

		Database.Connect();

        while (true) 
		{
            if (Console.ReadLine() == "Stop")
			{
				Server.Stop();
				break;
			}
		}
	}
}