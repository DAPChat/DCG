﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client
{
	public int id;
	public int gameId;
	public TCP tcp;

	public Client(int _id)
	{
		// Create a new TCP that manages all client functions
		id = _id;

        // Pass the Client class so variables are accessible in TCP
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
			
			// Start reading the stream
			stream = client.GetStream();
			buffer = new byte[1028];

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
		}

        public void WriteStream(byte[] _msg)
        {
			// Write the message to the stream to the correct client
			stream.BeginWrite(_msg, 0, _msg.Length, null, null);
        }

        private void ReadCallback(IAsyncResult _result)
		{
			try
			{
				int _readBytesLength = stream.EndRead(_result);

				// Check if the client disconnected
				// (the TCP sends a packet of length 0 on disconnect)
				if (_readBytesLength <= 0)
				{
					// Disconnects from server if not in game
					// Disconnect from game if in game
					if (instance.gameId == 0)
						Server.Disconnect(id);
					else
						Server.games[instance.gameId].LeaveGame(id);
					return;
				}

				if (instance.gameId != 0) Server.games[instance.gameId].Manage((byte[])buffer.Clone(), id);

				buffer = new byte[buffer.Length];

				stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, null);
			}catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.TargetSite);

				if (instance.gameId == 0)
					Server.Disconnect(id);
				else
					Server.games[instance.gameId].LeaveGame(id);
			}
		}

		public void Disconnect()
		{
			// Closes the client and stream
			stream.Close();
			client.Close();

			stream.Dispose();
			client.Dispose();

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