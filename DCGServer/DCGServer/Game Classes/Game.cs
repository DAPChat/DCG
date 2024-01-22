using player;
using packets;
using card;
using System;

namespace game
{
    public class Game
	{
		public int id;

		// Store the clients in the game with their id
		public Dictionary<int, Client> clients = new();
		public List<int> clientIds = new();

		public GameBoard currentBoard;

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

			p.username = _client.Acc().username;

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

            var field = action.card.Class == "Spell" ? player.fieldRowTwo : player.fieldRowOne;

            if (field[action.targetSlot] != null) return;

			action.card.Instantiate();

            field.SetValue(action.card, action.targetSlot);

			if (action.action == "place")
			{
                clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(new CAP { card = action.card, action = "hremove" }));
                player.hand.Remove(action.card.Id);
            }

			currentBoard.UpdatePlayer(player);

            action.action = "place";

			// Console.WriteLine(action.card);
			SendAll(PacketManager.ToJson(action));
		}

		public void RegisterAction(CAP action)
		{
			if (action == null) return;

            if (action.placerId != currentBoard.turn) return;

			if (action.action == "place" || action.action == "summon")
			{
				if (currentBoard.phase == 1 || action.action == "summon")
				{
					ActionManager.Register(action, this);
				}
			}
			else if (currentBoard.phase == 2)
				ActionManager.Register(action, this);

			if (!active) return;

            ActionManager.UpdateCards(currentBoard.GetPlayer(action.placerId).fieldRowOne, action.placerId, this);
            ActionManager.UpdateCards(currentBoard.GetPlayer(OpponentId(action.placerId)).fieldRowOne, OpponentId(action.placerId), this);
        }

		public void Damage(CAP action, int? dmg = null)
		{
			Player p = currentBoard.GetPlayer(OpponentId(action.placerId));

			if (!p.fieldRowOne[action.targetSlot].EffectName.Contains("Immortal"))
				p.fieldRowOne[action.targetSlot].Hp -= dmg == null? action.card.Atk : (int)dmg;

            currentBoard.UpdatePlayer(p);
		}

		public void PlayerDamage(Player p, int by)
		{
			p.lifePoints += by;

            SendAll(PacketManager.ToJson(new PUP { action = "uhp", player = p.Client() }));

            if (p.lifePoints <= 0)
            {
				PlayerManager.Win(clients[OpponentId(p.id)].Acc(), clients[p.id].Acc(), this);
                return;
            }

			currentBoard.UpdatePlayer(p);
        }

		public void RemoveCard(CAP action, TempCard[] field, int id, int slot)
		{
            Player p = currentBoard.GetPlayer(id);

            CAP _action = action.Clone();
			_action.targetSlot = slot;
            _action.targetId = p.id;
            _action.card = field[slot].MakeReady();
			_action.action = "remove";

            field.SetValue(null, slot);

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

		public bool Cost(BaseCard card, string stat, int price)
		{
            TempCard curCard = currentBoard.GetPlayer(card.action.placerId).fieldRowOne[card.action.senderSlot];

			int curNum = (int)curCard.GetType().GetProperty(stat).GetValue(curCard);

			if (curNum - price < 0) return false;

			curCard.GetType().GetProperty(stat).SetValue(curCard, curNum - price);

            SendAll(PacketManager.ToJson(new CAP { action = "update", targetId = card.action.placerId, card = curCard.MakeReady(), targetSlot = card.action.senderSlot }));

			return true;
        }
       
		public void AddStatus(BaseCard card, string name, int length)
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

			clients[card.action.placerId].tcp.WriteStream(PacketManager.ToJson(new EUP { type = "status", targetId = card.action.placerId, name = name, card = card.action.card.MakeReady(), slot = card.action.senderSlot }));
		}

		public void AddEffect(BaseCard card, string name, int length, int param)
		{
            TempCard curCard = currentBoard.GetPlayer(card.action.placerId).fieldRowOne[card.action.senderSlot];

            if (curCard == null) return;

            if (curCard.EffectName.Contains(name))
            {
                curCard.EffectLength[curCard.EffectName.IndexOf(name)] += length;
            }
            else
            {
                curCard.EffectName.Add(name);
                curCard.EffectLength.Add(length);
                curCard.EffectParam.Add(param);

				if (name != "Immortal")
					curCard.GetType().GetProperty(name).SetValue(curCard, (int)curCard.GetType().GetProperty(name).GetValue(curCard) + param);
            }

            SendAll(PacketManager.ToJson(new CAP { action = "update", targetId = card.action.placerId, card = curCard.MakeReady(), targetSlot = card.action.senderSlot }));

			clients[card.action.placerId].tcp.WriteStream(PacketManager.ToJson(new EUP { type = "effect", targetId = card.action.placerId, name = name, param = param, card = curCard.MakeReady(), slot = card.action.senderSlot }));
        }

		public void ResetStats(int id, int slot, string[] disclude)
		{
			Player p = currentBoard.GetPlayer(id);
			TempCard card = p.fieldRowOne[slot];
			TempCard matchCard = Database.GetCard(card.Id).TempCard();
			
			if (!disclude.Contains("Hp"))
				card.Hp = matchCard.Hp;
			if (!disclude.Contains("Atk"))
				card.Atk = matchCard.Atk;
			if (!disclude.Contains("Mana"))
				card.Mana = matchCard.Mana;

            SendAll(PacketManager.ToJson(new CAP { action = "update", targetId = p.id, card = p.fieldRowOne[slot].MakeReady(), targetSlot = slot }));
        }

		public void RemoveEffect(int slot, int effect, int id)
		{
			Player p = currentBoard.GetPlayer(id);
			TempCard card = p.fieldRowOne[slot];

			if (card.EffectName[effect] == "Immortal") return;

			switch (card.EffectName[effect])
			{
				case "Hp":
					CAP cap = new CAP
					{
						targetSlot = slot,
						card = card,
						placerId = OpponentId(id)
					};

                    Damage(cap, card.EffectParam[effect]);
					break;
				case "Atk":
					card.Atk -= card.EffectParam[effect];
					break;
				case "Mana":
					card.Mana -= card.EffectParam[effect];
					break;
			}

			CAP _action = new CAP
			{
				targetId = p.id,
				targetSlot = slot,
				card = p.fieldRowOne[slot].MakeReady(),
				action = "update"
			};

			SendAll(PacketManager.ToJson(_action));
        }

        public class GameBoard
		{
			Game game;

			public bool gameState;
			public int turn; // Stores the player id of who's turn it is
			public int phase;
			public int round;

			public int summoned;

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

							game.ResetStats(p.id, i, new string[] { "Hp", "Atk" });

                            if (p.fieldRowOne[i].StatusLength != null)
							{
								for (int e = p.fieldRowOne[i].StatusLength.Count - 1; e >= 0; e--)
								{
									if (p.fieldRowOne[i].StatusLength[e] != -1)
										p.fieldRowOne[i].StatusLength[e] -= 1;

									if (p.fieldRowOne[i].StatusLength[e] == 0)
									{
                                        p.fieldRowOne[i].StatusLength.RemoveAt(e);
                                        p.fieldRowOne[i].StatusName.RemoveAt(e);

                                        continue;
									}

									game.clients[p.id].tcp.WriteStream(PacketManager.ToJson(new EUP { type = "status", targetId = p.id, name = p.fieldRowOne[i].StatusName[e], card = p.fieldRowOne[i].MakeReady(), slot = i }));
								}
							}

							if (p.fieldRowOne[i].EffectLength != null)
							{
								for (int e = p.fieldRowOne[i].EffectLength.Count -1; e >= 0; e--)
								{
									if (p.fieldRowOne[i].EffectLength[e] != -1)
										p.fieldRowOne[i].EffectLength[e] -= 1;

									if (p.fieldRowOne[i].EffectLength[e] == 0)
									{
										game.RemoveEffect(i, e, p.id);

                                        p.fieldRowOne[i].EffectParam.RemoveAt(e);
                                        p.fieldRowOne[i].EffectName.RemoveAt(e);
                                        p.fieldRowOne[i].EffectLength.RemoveAt(e);

                                        continue;
									}

									game.clients[p.id].tcp.WriteStream(PacketManager.ToJson(new EUP { type = "effect", targetId = p.id, slot = i, name = p.fieldRowOne[i].EffectName[e], param = p.fieldRowOne[i].EffectParam[e], card = p.fieldRowOne[i].MakeReady() }));
								}
							}

                            game.SendAll(PacketManager.ToJson(new CAP { action = "update", targetId = p.id, card = p.fieldRowOne[i].MakeReady(), targetSlot = i }));
                        }

						ActionManager.UpdateCards(p.fieldRowOne, p.id, game);

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

				summoned = 0;
				
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