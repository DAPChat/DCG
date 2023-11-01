using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

class Server
{
	private static TcpListener tcpListener;
	
	public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

	public static void Start()
	{
		Console.Write("Starting Server...");

		tcpListener = new TcpListener(IPAddress.Any, 5001);

		tcpListener.Start();
		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);

		Console.WriteLine("Started");
	}

	private static void ClientAcceptCallback(IAsyncResult result)
	{
		TcpClient _client = tcpListener.EndAcceptTcpClient(result);

		int currentClientId = clients.Count + 1;

		clients.Add(currentClientId, new Client(currentClientId));
		clients[currentClientId].tcp.Connect(_client);

		Console.WriteLine($"Client connected to client with id: {0}", currentClientId);

		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);
	}

	public static void Disconnect(int id)
	{
		clients[id].tcp.Disconnect();
		clients.Remove(id);

		Console.WriteLine($"Disconnected from client with id: {0}", id);
	}
}