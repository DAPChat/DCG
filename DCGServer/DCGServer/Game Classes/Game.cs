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

			p.username = _client.Username();

			for (int i = 0; i < 6; i++)
			{
				string card = p.deck[new Random().Next(0, p.deck.Count - 1)];

				p.hand.Add(card);
				p.deck.Remove(card);
			}

			// Send to client (not including the entire deck)
			_client.tcp.WriteStream(PacketManager.ToJson(new Player { username = p.username, id = p.id, hand = p.hand }));

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

				Server.KeepConnect(client.Value);
			}

			clients.Clear();
			clientIds.Clear();

			Server.RemoveGame(id);
		}

		public void PlaceCard(CAP action)
		{
			if (!active) return;

			Player player = currentBoard.GetPlayer(action.placerId);

			if (action.action == "place" && !player.hand.Contains(action.card.Id)) return;
			else if (action.action == "summon" && !player.forgotten.Contains(action.card.Id)) return;

            var field = action.card.Class == "Spell" ? player.fieldRowTwo : player.fieldRowOne;

            if (field[action.targetSlot] != null) return;

			if (action.action == "place")
				player.hand.Remove(action.card.Id);
			else player.forgotten.Remove(action.card.Id);

			action.card.Instantiate();

            field.SetValue(action.card, action.targetSlot);

			currentBoard.UpdatePlayer(player);

			if (action.action == "place")
			{
                clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(new CAP { card = action.card, action = "hremove" }));
                currentBoard.NextPhase();
			}

            action.action = "place";

			// Console.WriteLine(action.card);
			SendAll(PacketManager.ToJson(action));
		}

		public void RegisterAction(CAP action)
		{
			if (action == null) return;

            if (action.placerId != currentBoard.turn) return;

			if (action.action == "place" || action.action == "summon")
				if (currentBoard.phase == 1 || action.action == "summon")
					PlaceCard(action);
				else return;
			else if (currentBoard.phase == 2)
				ActionManager.Register(action, this);
		}

		public void Damage(CAP action, object o)
		{
			Player p = currentBoard.GetPlayer(OpponentId(action.placerId));

			p.fieldRowOne[action.targetSlot].Hp -= action.card.Atk;

			CAP _action = action.Clone();
			_action.targetId = p.id;
			_action.card = p.fieldRowOne[_action.targetSlot].MakeReady();
			_action.action = "update";

            if (p.fieldRowOne[action.targetSlot].Hp <= 0)
			{
                p.lifePoints += p.fieldRowOne[action.targetSlot].Hp;

				SendAll(PacketManager.ToJson(new PUP { action = "uhp", player = p.Client() }));

                ((BaseCard)o).Death();
				_action.action = "remove";

				if (p.lifePoints <= 0)
				{
					Console.WriteLine(clients[action.placerId].Username() + " has won the battle!");
					Close();
					return;
				}
			}

            currentBoard.UpdatePlayer(p);

            SendAll(PacketManager.ToJson(_action));
		}

		public void SendAll(byte[] msg)
		{
			if (!active) return;
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

		public void AddEffect(BaseCard card, string name, int length)
		{
			TempCard curCard = currentBoard.GetPlayer(card.action.placerId).fieldRowOne[card.action.senderSlot];

			if (curCard == null) return;

			if (curCard.StatusName.Contains(name))
			{
				curCard.StatusLength[curCard.StatusName.IndexOf(name)] += length;
			}
			else
			{
				curCard.StatusName.Add(name);
				curCard.StatusLength.Add(length);
			}
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

						for (int i = 0; i < p.fieldRowOne.Length; i++)
						{
							if (p.fieldRowOne[i] == null) continue;
							if (p.fieldRowOne[i].StatusLength == null) continue;
							for (int e = 0; e < p.fieldRowOne[i].StatusLength.Count; e++)
							{
								if (p.fieldRowOne[i].StatusLength[e] == -1) continue;

								p.fieldRowOne[i].StatusLength[e] -= 1;

								if (p.fieldRowOne[i].StatusLength[e] <= 0)
								{
									p.fieldRowOne[i].StatusLength.RemoveAt(e);
									p.fieldRowOne[i].StatusName.RemoveAt(e);
								}
							}
						}

						Client placer = game.clients[turn];

                        if (p.deck.Count > 0 && p.hand.Count < 10)
                        {
                            string c = p.deck[new Random().Next(0, p.deck.Count)];

                            p.deck.Remove(c);
                            p.hand.Add(c);

							placer.player = p.Clone();
                            placer.tcp.WriteStream(PacketManager.ToJson(new CAP { card = Database.GetCard(c).TempCard(), action = "hadd" }));
                        }

						round++;

						NextPhase();

                        break;
					}
				}
                return turn;
			}

			public int NextPhase()
			{
				if (!game.active) return 0;

				if (phase < 2 && round != 1) phase++;
				else if (round == 1 && phase < 1) phase++;
				else NextTurn();

                game.SendAll(PacketManager.ToJson(new GSP { gameId = game.id, turn = turn, phase = phase }));

                return phase;
			}
		}
	}
}