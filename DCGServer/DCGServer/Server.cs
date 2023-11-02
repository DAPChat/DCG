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

		CheckMatch();

		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);
	}

	private static void CheckMatch()
	{
		if (tempClient.Count >= 2)
		{
			int gameId = 1;

			while(games.ContainsKey(gameId))
			{
				gameId++;
			}

			List<Client> clientsToAdd = new List<Client>();

			for (int i = 0; i < 2; i++)
			{
				clientsToAdd.Add(tempClient.First().Value);
				tempClient.Remove(tempClient.First().Key);
			}

			Game game = new Game(gameId, clientsToAdd);

			games.Add(gameId, game);

			return;
		}
	}

	public static void LeaveGame(int _gameId, int _clientId)
	{
		games[_gameId].Disconnect(_clientId);
		ids.Remove(_clientId);
		playerCount--;

        Console.WriteLine("Client Removed! With ID {0} on server {1}", _clientId, _gameId);

        games.Remove(_gameId);
	}

	public static void Queue(int _id, Client _client)
	{
		tempClient.Add(_id, _client);

		Console.WriteLine("Client Added To Queue! Id: {0}", _id);
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