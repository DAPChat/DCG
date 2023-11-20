using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

class Server
{
	private static TcpListener tcpListener;

    public static int playerCount = 0;
	// List of all active ids to negate duplicates
    public static List<int> ids = new List<int>();

	// Active games and their ids
    public static Dictionary<int, Game> games = new Dictionary<int, Game>();
	// Players queued to join a game
	public static Dictionary<int, Client> tempClient = new Dictionary<int, Client>();

	public static void Start()
	{
		Console.Write("Starting Server...");

		tcpListener = new TcpListener(IPAddress.Any, 60606);

		// Starts server and looks for incoming clients
		tcpListener.Start();
		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);

		Console.WriteLine("Started");
	}

	public static void Stop()
	{
		foreach (Game game in games.Values)
		{
			game.Close();
		}

		tcpListener.Stop();
	}

	private static void ClientAcceptCallback(IAsyncResult result)
	{
		TcpClient _client = new TcpClient();

		try
		{
			_client = tcpListener.EndAcceptTcpClient(result);
		}catch (Exception ex)
		{
		}

		// On connection, increase playercount
		playerCount++;

		int _currentClientId = 1;

		// Create a unique id for the player
		while(ids.Contains(_currentClientId))
		{
			_currentClientId++;
		}

		ids.Add(_currentClientId);
		tempClient.Add(_currentClientId, new Client(_currentClientId));

		// Tell the client to connect to player
		tempClient[_currentClientId].tcp.Connect(_client);

		Player p = new(_currentClientId);

		byte[] msg = PacketManager.ToJson(p);

		tempClient[_currentClientId].tcp.WriteStream(msg);

		Console.WriteLine($"Client connected with id: {_currentClientId}, {playerCount} player(s) online!");

		// Check if there is an available player to join game
		CheckMatch();

		// Listen for new player
		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);
	}

	private static void CheckMatch()
	{
		// Check if there are enough players to create a game (2)
		// Create a new match and add the clients to the game if possible
		if (tempClient.Count >= 2)
		{
			// Create unique game id
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

			// Create a new game and add the clients
			Game game = new Game(gameId);

			games.Add(gameId, game);

			game.AddClients(clientsToAdd);

			return;
		}
	}

	public static void Queue(int _id, Client _client)
	{
		// Add a client back to a queue if the game closed (called from Game class)
		// Add more functionality later
		_client.gameId = 0;

		GSP gsp = new();
		gsp.gameId = 0;
		gsp.senderId = _id;

		_client.tcp.WriteStream(PacketManager.ToJson(gsp));

		tempClient.Add(_id, _client);

		Console.WriteLine("Client Added To Queue! Id: {0}", _id);

		CheckMatch();
	}

	public static void RemoveGame(int _gameId)
	{
		games.Remove(_gameId);
	}

	public static void Disconnect(int id)
	{
		// Remove the client from the server if they are not in game
		// Free up the id from the server
		ids.Remove(id);
		tempClient[id].tcp.Disconnect();
		tempClient.Remove(id);

		playerCount--;

		Console.WriteLine($"Disconnected from client with id: {id}, {playerCount} player(s) remain!");
	}
}