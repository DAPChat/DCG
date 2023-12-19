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
			string input = Console.ReadLine();

			if (input == null) continue;

            if (input.ToLower() == "stop")
			{
				Server.Stop();
				break;
			}
			
			if (input.ToLower() == "reset")
			{
				Database.ResetAccounts();
			}
		}
	}
}