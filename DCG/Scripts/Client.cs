using Godot.Collections;
using Godot;
using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class Client
{
	public TcpClient client = null;
	public PlayerAccount account = null;

	private IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.1.1.0"), 60606);

	private NetworkStream stream = null;
	private byte[] buffer = new byte[4096];

	public Dictionary<string, string> values = new Dictionary<string, string>();
	public bool connected = false;

	public int id;
	public int gameId;

	public Player player;

    CancellationTokenSource cts = new CancellationTokenSource();

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
			//client.BeginConnect(endPoint.Address, endPoint.Port, ConnectCallback, client);
			TryConnect();
		}
		catch (Exception e)
		{
			Connect();
		}
	}

	private async Task TryConnect()
	{
        if (client == null)
        {
            client = new TcpClient();
        }

        while (!client.Connected)
		{
			cts = new CancellationTokenSource();
			cts.CancelAfter(5000);

            try
			{
				await client.ConnectAsync(endPoint.Address, endPoint.Port, cts.Token);
			}
			catch (Exception e) { }
			finally { cts.Cancel(); }
		}

		ConnectCallback();
    }

    public void WriteStream(byte[] _msg)
    {
        // Write the message to the stream to the correct client
        stream.BeginWrite(_msg, 0, _msg.Length, null, null);
    }

    private void ConnectCallback() //IAsyncResult result)
	{
		// client.EndConnect(result);

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
				Disconnect();
				return;
			}

			PacketManager.Decode(buffer, this);

			//byte[] b = Encoding.ASCII.GetBytes("[Packet]{\"type\":\"CSP\", \"senderId\":" + id + ", \"parameters\": '{\"time\":\"" + DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "\"}'}");

			//if(gameId > 0)
			//	stream.BeginWrite(b, 0, b.Length, null, null);

			buffer = new byte[buffer.Length];

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
		}
		catch (Exception e)
		{
			connected = false;
			Disconnect();
		}
	}

	private void Disconnect()
	{
        stream.Close();
        client.Close();

		client = null;
		stream = null;

        account = null;

        if (gameId != 0)
		{
			gameId = 0;
			GameScene.changeScene = true;
		}
		else
		{
			Main.Reload();
		}

		TryConnect();
	}
}
