using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

class Server
{
	private static TcpListener tcpListener;

	public static bool on = false;
    public static int playerCount = 0;
	// List of all active ids to negate duplicates
    public static List<int> ids = new List<int>();

	// Active games and their ids
    public static Dictionary<int, Game> games = new Dictionary<int, Game>();
	// Players queued to join a game
	public static Dictionary<int, Client> queue = new Dictionary<int, Client>();

	public static List<Client> clients = new List<Client>();

	public static void Start()
	{
		Console.Write("Starting Server...");

		on = true;

		tcpListener = new TcpListener(IPAddress.Any, 60606);

		// Starts server and looks for incoming clients
		tcpListener.Start();
		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);

		Console.WriteLine("Started");
	}

	public static void Stop()
	{
		on = false;

        tcpListener.Stop();

		// Disconnects every client and closes every game
        foreach (Game game in games.Values)
		{
			game.Close();
		}

		foreach (Client client in clients)
		{
			client.Disconnect();
		}

		foreach (Client client in queue.Values)
		{
			client.Disconnect();
		}
	}

	private static void ClientAcceptCallback(IAsyncResult result)
	{
		if (!on) return;

		TcpClient _client = new TcpClient();

		try
		{
			_client = tcpListener.EndAcceptTcpClient(result);
		}catch (Exception ex)
		{
		}

		// On connection, increase playercount
		playerCount++;

        int _id = 1;

        // Create a unique id for the player
        while (ids.Contains(_id))
        {
            _id++;
        }

        ids.Add(_id);
        clients.Add(new Client(_id));

        Console.WriteLine($"Client connected with id: {_id}, {playerCount} player(s) online!");

        clients.Last().tcp.Connect(_client);

		// Listen for new player
		tcpListener.BeginAcceptTcpClient(ClientAcceptCallback, null);
	}

    private static void CheckMatch()
	{
		// Check if there are enough players to create a game (2)
		// Create a new match and add the clients to the game if possible
		if (queue.Count >= 2)
		{
			// Create unique game id
			int gameId = 1;

			while (games.ContainsKey(gameId))
			{
				gameId++;
			}

			List<Client> clientsToAdd = new List<Client>();

			for (int i = 0; i < 2; i++)
			{
				clientsToAdd.Add(queue.First().Value);
				queue.Remove(queue.First().Key);
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
		// Add a client to the queue
		_client.gameId = 0;

		GSP gsp = new();
		gsp.gameId = 0;
		gsp.senderId = _id;

		_client.tcp.WriteStream(PacketManager.ToJson(gsp));

		queue.Add(_id, _client);

        Console.WriteLine("Client Added To Queue! Id: {0}", _id);

		CheckMatch();
	}

	public static void KeepConnect(Client _client)
	{
		// Keeps a client connected (if they leave queue or leave match)
        _client.gameId = 0;

        GSP gsp = new();
        gsp.gameId = 0;
        gsp.senderId = _client.id;

        _client.tcp.WriteStream(PacketManager.ToJson(gsp));

        Server.clients.Add(_client);
    }

	// Remove a game from the server
    public static void RemoveGame(int _gameId)
	{
		games.Remove(_gameId);
	}

	public static void Disconnect(int id, int gameId, Client _client)
	{
		// Remove the client from the server if they are not in game
		// Free up the id from the server
		ids.Remove(id);

		if (queue.ContainsKey(id))
		{
			queue[id].tcp.Disconnect();
			queue.Remove(id);
		}
		else if (clients.Contains(_client))
		{
			clients[clients.IndexOf(_client)].Disconnect();
			clients.Remove(_client);
		}

		playerCount--;

        Console.WriteLine($"Disconnected from client with id: {id}, {playerCount} player(s) remain!");
	}
}