using Godot;
using System;
using System.Net.Sockets;
using System.Text;

public partial class ServerManager : Godot.Node
{
	public Label label;

	private TcpClient client = null;
	private NetworkStream stream = null;
	private byte[] buffer = new byte[1024];

	private string str = null;

	public override void _Ready()
	{
		if (client == null)
		{
			client = new TcpClient();
		}

		label = GetNode<Label>("/root/ServerManager/CanvasLayer/Label");

<<<<<<< Updated upstream
		client.BeginConnect("10.72.100.135", 5001, ConnectCallback, client);
=======
        // School IP: 10.72.100.135

        client.BeginConnect("127.1.1.0", 5001, ConnectCallback, client);
>>>>>>> Stashed changes
	}

	private void ConnectCallback(IAsyncResult result)
	{
		client.EndConnect(result);

		stream = client.GetStream();

		// stream.Write(Encoding.ASCII.GetBytes("Here"), 0, 4);

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

			str = "ID: " + Encoding.ASCII.GetString(buffer);

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

	public override void _Process(double delta)
	{
		if(str != null)
		{
			label.Text = str;
			str = null;
		}
	}
}
