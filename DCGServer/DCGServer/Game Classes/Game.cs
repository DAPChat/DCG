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

		Console.Write(id + ": ");
		Console.WriteLine("Client Added! With ID {0} on server {1}", currentClientId, id);


		// Update the client's information
		_client.gameId = id;

		// Create a new player and save it to both the server and send to client
		Player p = new(_client.id) { playerNum = clients.Count };

        _client.player = p;

        p.deck = _client.ActiveDeck();

		// Send to client (not including the entire deck)
        _client.tcp.WriteStream(PacketManager.ToJson(_client.player.Client()));

        GSP gsp = new()
        {
            gameId = id,
            senderId = _client.id
        };

		currentBoard.AddPlayer(_client.player);

		byte[] msg = PacketManager.ToJson(gsp);

		// Send the game id to the client
		_client.tcp.WriteStream(msg);
    }

	public void Manage(byte[] data, int _clientId)
	{
		if (!active) return;

		PacketManager.Decode(data, clients[_clientId]);
	}

	public void LeaveGame(int _clientId)
	{
		active = false;

		// Remove the disconnected client from the server
		clients[_clientId].Disconnect();
		clients.Remove(_clientId);
		Server.ids.Remove(_clientId);

		Server.playerCount--;

		Console.WriteLine("Client Removed! With ID {0} on server {1}, {2} player(s) remain!", _clientId, id, Server.playerCount);

		if (clients.Count < 1)
		{
			Close();
			return;
		}

		// Remove the non-disconnected person from the match
		Server.KeepConnect(clients.Values.First());

		Server.RemoveGame(id);
	}

	public void Close()
	{
		// Close the game and disconnect all clients
		active = false;

		foreach (var client in clients)
		{
			client.Value.gameId = 0;

			Server.ids.Remove(client.Key);

			client.Value.Disconnect();
		}

		clients.Clear();
		clientIds.Clear();

		Server.RemoveGame(id);
	}

	class GameBoard
	{
		bool gameState;
		int turn;
		int phase;
		int round;

		string image;

		Player player1;
		Player player2;

		public GameBoard(Game _game)
		{
			gameState = true;
			turn = 0;
			phase = 0;
			round = 1;

			image = null;
		}

		public void AddPlayer(Player player)
		{
			if (player1 == null)
			{
				player1 = player;
			}
			else if (player2 == null)
			{
				player2 = player;
			}
		}

		// Add the players to their respective positions
		public void AddPlayers(List<Player> players)
		{
			foreach (var player in players)
			{
				AddPlayer(player);
			}
		}
	}
}