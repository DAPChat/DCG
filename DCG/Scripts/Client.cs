using System;
using System.Net.Sockets;
using System.Text;

public partial class Client
{
	private static TcpClient client = null;
	private static NetworkStream stream = null;
	private static byte[] buffer = new byte[1024];

	public static string str = null;
	public static bool connected = false;

	public static void Connect()
	{
		if(connected) return;

		// Try to connect to the client
		// If it fails, try again
		try
		{
			if (client == null)
			{
				client = new TcpClient();
			}

            client.BeginConnect("127.1.1.0", 5001, ConnectCallback, client);
		}catch (Exception e)
		{
			Connect();
		}
    }


	private static void ConnectCallback(IAsyncResult result)
	{
		client.EndConnect(result);

		connected = true;

		stream = client.GetStream();

		// Read the incoming messages
		stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
	}

	private static void ReadCallback(IAsyncResult result)
	{
		try
		{
			int _bytesRead = stream.EndRead(result);

			// Check if the server stops
			if (_bytesRead <= 0)
			{
				connected = false;
				return;
			}

			str = "ID: " + Encoding.ASCII.GetString(buffer);

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
		}
		catch (Exception e)
		{
			connected = false;
		}
	}
}
