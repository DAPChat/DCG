using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

class Server
{
	private static TcpListener tcpListener;
	
	public static Dictionary<int, Game> games = new Dictionary<int, Game>();
	public static Dictionary<int, Client> tempClient = new Dictionary<int, Client>();

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

		int currentClientId = tempClient.Count + 1;

		tempClient.Add(currentClientId, new Client(currentClientId));

		tempClient[currentClientId].tcp.Connect(_client);

		Console.WriteLine($"Client connected to client with id: {currentClientId}");

		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);
	}

	public static void Disconnect(int id)
	{
		tempClient[id].tcp.Disconnect();
		tempClient.Remove(id);

		Console.WriteLine($"Disconnected from client with id: {id}");
	}
}