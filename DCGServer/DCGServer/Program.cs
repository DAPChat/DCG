using System.Net;
using System.Net.Sockets;

class Program
{
    static BaseClass c = new BaseClass();

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