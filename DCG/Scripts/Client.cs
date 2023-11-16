using Godot.Collections;
using Godot;
using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

public class Client
{
	public TcpClient client = null;

	private IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.1.1.0"), 5001);

	private NetworkStream stream = null;
	private byte[] buffer = new byte[1028];

	public Dictionary<string, string> values = new Dictionary<string, string>();
	public bool connected = false;

	public int id;
	public int gameId;

	public void Connect()
	{
		if (connected) return;

		// Try to connect to the client
		// If it fails, try again
		try
		{
			if (client == null)
			{
				client = new TcpClient();
			}
			// 127.1.1.0
			client.BeginConnect(endPoint.Address, endPoint.Port, ConnectCallback, client);
		}
		catch (Exception e)
		{
			Connect();
		}
	}

	private void ConnectCallback(IAsyncResult result)
	{
		client.EndConnect(result);

		connected = true;

		stream = client.GetStream();

		// Read the incoming messages
		stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
	}

	private void ReadCallback(IAsyncResult result)
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

			PacketManager.Decode(buffer, this);

			byte[] b = Encoding.ASCII.GetBytes("[Packet]{\"type\":\"CSP\", \"senderId\":" + id + ", \"parameters\": '{\"time\":\"" + DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "\"}'}");

			//ServerManager.Print(Encoding.ASCII.GetString(buffer));

			if(gameId > 0)
				stream.BeginWrite(b, 0, b.Length, null, null);

			buffer = new byte[buffer.Length];

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
		}
		catch (Exception e)
		{
			connected = false;
		}
	}
}
