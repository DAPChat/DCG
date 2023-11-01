using System;
using System.Net.Sockets;
using System.Text;

class Client
{
	public int id;
	public TCP tcp;

	public Client(int _id)
	{
		id = _id;

		tcp = new TCP(id);
	}

	public class TCP
	{
		public TcpClient client;

		private readonly int id;
		private NetworkStream stream;
		private byte[] buffer;

		public TCP(int _id)
		{
			id = _id;
		}

		public void Connect(TcpClient _client)
		{
			client = _client;
			
			stream = client.GetStream();
			buffer = new byte[4096];

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
		}

		private void ReadCallback(IAsyncResult _result)
		{
			try
			{
				int _readBytesLength = stream.EndRead(_result);

				if (_readBytesLength <= 0)
				{
					Server.Disconnect(id);
					return;
				}

				Console.WriteLine(Encoding.ASCII.GetString(buffer));

				stream.WriteAsync(Encoding.ASCII.GetBytes(id.ToString()));

				stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
			}catch (Exception e)
			{
				Server.Disconnect(id);
			}
		}

		public void Disconnect()
		{
			stream.Close();
			client.Close();

			client = null;
			stream = null;

			buffer = null;
		}
	}

	public void Disconnect()
	{
		tcp.Disconnect();
	}
}