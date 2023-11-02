using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

class Server
{
	private static TcpListener tcpListener;
	
	public static Dictionary<int, Game> games = new Dictionary<int, Game>();
	public static Dictionary<int, Client> tempClient = new Dictionary<int, Client>();

	private static int playerCount = 0;
	private static List<int> ids = new List<int>();

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

		playerCount++;

		int _currentClientId = 1;

		while(ids.Contains(_currentClientId))
		{
			_currentClientId++;
		}

		ids.Add(_currentClientId);
		tempClient.Add(_currentClientId, new Client(_currentClientId));

		tempClient[_currentClientId].tcp.Connect(_client);

		Console.WriteLine($"Client connected to client with id: {_currentClientId}");

		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);
	}

	public static void Disconnect(int id)
	{
		ids.Remove(id);
		tempClient[id].tcp.Disconnect();
		tempClient.Remove(id);

		playerCount--;

		Console.WriteLine($"Disconnected from client with id: {id}");
	}
}