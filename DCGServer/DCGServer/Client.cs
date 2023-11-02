using System;
using System.Net.Sockets;
using System.Text;

public class Client
{
	public int id;
	public int gameId;
	public TCP tcp;

	public Client(int _id)
	{
		id = _id;

		tcp = new TCP(id, this);
	}

	public class TCP
	{
		public TcpClient client;
		public Client instance;

		private readonly int id;
		private NetworkStream stream;
		private byte[] buffer;

		public TCP(int _id, Client _client)
		{
			id = _id;
			instance = _client;
		}

		public void Connect(TcpClient _client)
		{
			client = _client;
			
			stream = client.GetStream();
			buffer = new byte[4096];

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
		}

        public void WriteStream(byte[] _msg)
        {
			stream.BeginWrite(_msg, 0, _msg.Length, null, null);
        }

        private void ReadCallback(IAsyncResult _result)
		{
			try
			{
				int _readBytesLength = stream.EndRead(_result);

				if (_readBytesLength <= 0)
				{
					if (instance.gameId == 0)
						Server.Disconnect(id);
					else
						Server.LeaveGame(instance.gameId, id);
					return;
				}

				Console.WriteLine(Encoding.ASCII.GetString(buffer));

				// WriteStream(Encoding.ASCII.GetBytes(id.ToString()));

				stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
			}catch (Exception e)
			{
				if (instance.gameId == 0)
					Server.Disconnect(id);
				else
					Server.LeaveGame(instance.gameId, id);
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