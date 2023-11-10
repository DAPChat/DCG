using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class Game
{
	public int id;

	// Store the clients in the game with their id
	public Dictionary<int, Client> clients = new Dictionary<int, Client>();
	public List<int> clientIds = new List<int>();

	GameBoard currentBoard = null;

	private bool active;

	public Game(int _id)
	{
		id = _id;

		active = true;
	}

	public Game(int _id, List<Client> _clients)
	{
		id = _id;

		active = true;

        currentBoard = new GameBoard(this);
    }

	public void AddClients(List<Client> _clients)
	{
		if (!active) return;
		// Ensures only 2 players can be in each game
		// Re-adds the clients to the queue to give them a second-chance
		if (_clients.Count > 2)
		{
			Console.WriteLine("Error Creating Match!");
			foreach (Client _client in _clients)
			{
				Server.Queue(_client.id, _client);
			}
			return;
		}

		// Add each client and update the client's data
		foreach (Client _client in _clients)
		{
			AddClient(_client);
		}
	}

	// Shouldn't be used... just for testing right now and opens for future concepts
	private void AddClient(Client _client)
	{
		if (!active) return;

		if(clients.Count >= 2)
		{
			Console.WriteLine("Error Joining Match!");
			Server.Queue(_client.id, _client);
			return;
		}

		int currentClientId = _client.id;
		clients.Add(currentClientId, _client);

		clientIds.Add(currentClientId);

		// _client.tcp.WriteStream(Encoding.ASCII.GetBytes(id.ToString()));

		Console.Write(id + ": ");
		Console.WriteLine("Client Added! With ID {0} on server {1}", currentClientId, id);

		_client.gameId = id;

		GSP gsp = new();
		gsp.gameId = id;
		gsp.senderId = _client.id;

		byte[] msg = Encoding.ASCII.GetBytes("[Packet]" + Encoding.ASCII.GetString(PacketManager.ToJson(gsp)));

		_client.tcp.WriteStream(msg);
	}

	public void Manage(byte[] data, int _clientId)
	{
		if (!active) return;

		PacketManager packetManager = new PacketManager();

		packetManager.Decode(data, clients[_clientId]);

		foreach (int i in clientIds)
		{
			if (i != _clientId)
			{
				clients[i].tcp.WriteStream(data);
			}
		}
	}

	public void LeaveGame(int _clientId)
	{
		active = false;

		clients[_clientId].Disconnect();
		clients.Remove(_clientId);
		Server.ids.Remove(_clientId);

		Server.playerCount--;

		Console.WriteLine("Client Removed! With ID {0} on server {1}, {2} player(s) remain!", _clientId, id, Server.playerCount);

		if (clients.Count < 1) return;

		Server.Queue(clients.First().Key, clients.First().Value);

		Server.RemoveGame(id);
	}

	class GameBoard
	{
		static bool gameState;
		static int turn;
		static int phase;
		static int round;

		static string image;

		static Player player1;
		static Player player2;

		public GameBoard(Game _game)
		{
			gameState = true;
			turn = 0;
			phase = 0;
			round = 1;

			image = null;

			player1 = new Player(_game.clientIds[0]);
			player2 = new Player(_game.clientIds[1]);
		}
	}
}