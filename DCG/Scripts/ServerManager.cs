using Godot;
using System;
using System.Net.Sockets;
using System.Text;

public partial class ServerManager : Godot.Node
{
	private TcpClient client = null;
	private NetworkStream stream = null;

	private byte[] buffer = new byte[1024];

	public override void _Ready()
	{
		if (client == null)
		{
			client = new TcpClient();
		}

		client.BeginConnect("127.0.0.1", 5001, ConnectCallback, client);
	}

	private void ConnectCallback(IAsyncResult result)
	{
		client.EndConnect(result);

		stream = client.GetStream();

		stream.Write(Encoding.ASCII.GetBytes("Hello"), 0, 5);

		stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
	}

	private void ReadCallback(IAsyncResult result)
	{
		try
		{
			int _bytesRead = stream.EndRead(result);

			if (_bytesRead <= 0)
			{
				GD.Print("Lost connection with the server!");
				return;
			}

			GD.Print(Encoding.ASCII.GetString(buffer));

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
		}
		catch (Exception e)
		{
			GD.Print("Lost connection with the server!");
		}
	}

	private void OnApplicationQuit()
	{
		if (client != null)
		{
			client.Close();
			if (stream != null)
			{
				stream.Close();
			}
		}

		client = null;
		stream = null;
	}
}
