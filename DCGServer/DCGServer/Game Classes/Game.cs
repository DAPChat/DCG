using player;
using packets;
using card;
using System.Numerics;
using System.Text;

namespace game
{
    public class Game
	{
		public int id;

		// Store the clients in the game with their id
		public Dictionary<int, Client> clients = new Dictionary<int, Client>();
		public List<int> clientIds = new List<int>();

		public GameBoard currentBoard = null;

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

			currentBoard.NextTurn();
		}

		private void AddClient(Client _client)
		{
			if (!active) return;

			if (clients.Count >= 2)
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
			if (!active && !clients[_clientId].active) return;

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
			if (clients.Count > 0 && clients.Values.First().active)
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

			var field = action.card.Type == "Spell" ? placer.player.fieldRowTwo : placer.player.fieldRowOne;

			if (field[action.slot - 1] != null) return;

            placer.player.hand.Remove(action.card.Id);

            field.SetValue(action.card, action.slot - 1);

			currentBoard.UpdatePlayer(placer.player);

			placer.tcp.WriteStream(PacketManager.ToJson(new CAP { card = action.card, action = "hremove" }));

			foreach (var client in clients.Values)
			{
				client.tcp.WriteStream(PacketManager.ToJson(action));
			}

			currentBoard.NextPhase();
		}

		public void RegisterAction(CAP action)
		{
			if (action == null) return;

            if (action.placerId != currentBoard.turn) return;

			if (action.action == "place" && currentBoard.phase == 1)
				PlaceCard(action);
			else if (currentBoard.phase == 2)
				ActionManager.GetClass(action, this);
		}

		public void Damage(CAP action)
		{
			Player p = currentBoard.GetPlayer(OpponentId(action.placerId));

			p.fieldRowOne[action.slot].Hp -= action.card.Atk;

			currentBoard.UpdatePlayer(p);

			CAP _action = action.Clone();
			_action.targetId = OpponentId(action.placerId);
			_action.card = p.fieldRowOne[_action.slot];
			_action.action = p.fieldRowOne[_action.slot].Hp > 0 ? "update" : "remove";

			Console.WriteLine(_action.action + _action.slot);

			if (p.fieldRowOne[action.slot].Hp <= 0) p.fieldRowOne.SetValue(null, action.slot);

			SendAll(PacketManager.ToJson(_action));
		}

		public void SendAll(byte[] msg)
		{
            foreach (var client in clients.Values)
            {
                client.tcp.WriteStream(msg);
            }
        }

		public int OpponentId(int id)
		{
			foreach (int i in clientIds)
			{
				if (i != id) return i;
			}

			return 0;
		}

		public class GameBoard
		{
			Game game;

			public bool gameState;
			public int turn; // Stores the player id of who's turn it is
			public int phase;
			public int round;

			string image;

			Player[] players = new Player[2];

			public GameBoard(Game _game)
			{
				gameState = true;
				turn = 0;
				phase = 0;
				round = 0;

				image = "";

				game = _game;
			}

			public void AddPlayer(Player player)
			{
				for (int i = 0; i < players.Length; i++)
				{
					if (players[i] == null)
					{
						players[i] = player;
						turn = player.id;
						return;
					}
				}
			}

			// Add the players to their respective positions
			public void AddPlayers(List<Player> _players)
			{
				for (int i = 0; i < _players.Count; i++)
				{
					AddPlayer(_players[i]);
				}

                game.SendAll(PacketManager.ToJson(new GSP { gameId = game.id, turn = turn, phase = phase }));
            }

			public Player GetPlayer(int id)
			{
				foreach (Player player in players)
				{
					if (player.id == id)
						return player;
				}
				return null;
			}

			public void UpdatePlayer(Player p)
			{
				for (int i = 0; i < players.Length; i++)
				{
					if (players[i].id == p.id)
					{
						players[i] = p;
						game.clients[players[i].id].player = p;
					}
				}
			}

			public int NextTurn()
			{
				foreach (var p in players)
				{
					if (p.id != turn)
					{
						phase = 0;
                        turn = p.id;

						Client placer = game.clients[turn];

                        if (p.deck.Count > 0 && p.hand.Count < 10)
                        {
                            string c = p.deck[new Random().Next(0, p.deck.Count)];

                            //p.deck.Remove(c);
                            p.hand.Add(c);

							placer.player = p.Clone();
                            placer.tcp.WriteStream(PacketManager.ToJson(new CAP { card = Database.GetCard(c).TempCard(), action = "hadd" }));
                        }

						round++;

						NextPhase();

                        break;
					}
				}

				//game.SendAll(PacketManager.ToJson(new GSP { gameId = game.id, turn = turn, phase = phase }));

                return turn;
			}

			public int NextPhase()
			{
				if (phase < 2 && round != 1) phase++;
				else if (round == 1 && phase < 1) phase++;
				else NextTurn();

                game.SendAll(PacketManager.ToJson(new GSP { gameId = game.id, turn = turn, phase = phase }));

                return phase;
			}
		}
	}
}