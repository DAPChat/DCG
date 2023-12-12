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
		p.hand = new();
		
		for (int i = 0; i < 6; i++)
		{
			string card = p.deck[new Random().Next(0, p.deck.Count - 1)];

			p.hand.Add(card);
			p.deck.Remove(card);
		}

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
		if (clients.Count > 0)
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

	public void PlaceCard(CAP action)
	{
		if (action.action != "place" || !active) return;

		Client placer = clients[action.placerId];

		if (!placer.player.hand.Contains(action.card.Id)) return;

		placer.player.hand.Remove(action.card.Id);

		var field = action.card.Type == "Spell" ? placer.player.fieldRowTwo : placer.player.fieldRowOne;

		if (field[action.slot - 1] != null) return;

		field.SetValue(action.card.Id, action.slot-1);

        currentBoard.UpdatePlayer(placer.player);

		placer.tcp.WriteStream(PacketManager.ToJson(new CAP { card = action.card, action = "hremove"}));

        foreach (var client in clients.Values)
		{
			client.tcp.WriteStream(PacketManager.ToJson(action));
		}
	}

	class GameBoard
	{
		bool gameState;
		int turn;
		int phase;
		int round;

		string image;

		List<Player> players = new();

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
			players.Add(player);
		}

		// Add the players to their respective positions
		public void AddPlayers(List<Player> _players)
		{
			for (int i = 0; i < players.Count; i++)
			{
				AddPlayer(_players[i]);
			}
		}

		public void UpdatePlayer(Player p)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].id == p.id)
				{
					players[i] = p;
				}
			}
		}
	}
}