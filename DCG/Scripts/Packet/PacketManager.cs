using Newtonsoft.Json;
using System;
using System.Text;

using packets;
using player;
using System.Linq;
using System.Collections.Generic;

public class PacketManager
{
	// Parses the incoming packets from the stream and runs functions depending on the packet type
	public static void Decode(byte[] _data, Client client)
	{
		string datapend = Encoding.UTF8.GetString(_data);

		string[] dataList = datapend.Split("[Packet]");

        for (int i = 1; i < dataList.Length; i++)
		{
			string data = dataList[i];

			try
			{
                LoadType lt = JsonConvert.DeserializeObject<LoadType>(data);

				switch (lt.type)
				{
					case "CSP":
						var csp = JsonConvert.DeserializeObject<CSP>(lt.parameters);

						break;

					case "GSP":
						// Updates the game-state and sets the scenes accordingly
						var gsp = JsonConvert.DeserializeObject<GSP>(lt.parameters);

						if (client.gameId != gsp.gameId)
						{
							client.gameId = gsp.gameId;
							Main.inGame = !Main.inGame ? true : false;

							if (!Main.inGame)
							{
								GameScene.changeScene = true;
							}

							break;
                        }

						if (gsp.end)
						{
							GameScene.winner = gsp.winner;
						}

						if (GameScene.currentTurn != gsp.turn && gsp.turn == ServerManager.client.id) GameScene.actionQueue.Add(new CAP { action = "ueffects" });

						// if (GameScene.zoomed != null) GameScene.ShowActions(GameScene.zoomed);

						GameScene.currentTurn = gsp.turn;
						GameScene.currentPhase = gsp.phase;

						if (gsp.turn == -1)
						{
							ServerManager.client.gameId = 0;
							GameScene.changeScene = true;
						}

						GameScene.actionQueue.Add(new CAP { action = "uturn" });

						break;

					case "Player":
						// Updates the player (includes decks and gameboard)
						var player = JsonConvert.DeserializeObject<Player>(lt.parameters);

						ServerManager.client.SetPlayer(player);
						ServerManager.client.id = player.id;

						break;

					case "PlayerAccount":
						// Signs in to the right account and keeps it saved until log-out
						var account = JsonConvert.DeserializeObject<PlayerAccount>(lt.parameters);

						client.account = account;

						break;

					case "Card":
						// Temporary function to recieve the card from the database and set it on the board
						var card = JsonConvert.DeserializeObject<GameScene.CardObject>(lt.parameters);

						GameScene.cardObject = card;

						break;

					case "CAP":
						// Tests placing cards from the database
						var cap = JsonConvert.DeserializeObject<CAP>(lt.parameters);

						GameScene.actionQueue.Add(cap);

						break;

					case "ACP":
						// Checks for successful login
						var login = JsonConvert.DeserializeObject<ACP>(lt.parameters);

						if (!login.create)
						{
							Main.Retry(login.error);
						}
						else
						{
							Main.Success();
						}

						break;
                    case "CRP":
                        // Tests placing cards from the database
                        var crp = JsonConvert.DeserializeObject<CRP>(lt.parameters);
                        //crp.main ? Editor.client.AddToHand(crp.card) : ServerManager.client.AddToHand(crp.card);
                        ServerManager.client.AddToHand(crp.card);

                        break;
					case "PUP":
						var pup = JsonConvert.DeserializeObject<PUP>(lt.parameters);

						GameScene.playerQueue.Add(pup);
						break;
					case "EUP":
						var eup = JsonConvert.DeserializeObject<EUP>(lt.parameters);

						GameScene.effectQueue.Add(eup);
						
						break;
                }
			}
			catch (Exception e)
			{
				ServerManager.Print(Encoding.UTF8.GetString(_data));
				ServerManager.Print($"{e}");
			}
		}
	}

	// Converts an object to Json with the proper formatting for parsing
    public static byte[] ToJson(object json)
    {
        LoadType loadType = new LoadType();

        loadType.parameters = JsonConvert.SerializeObject(json);
        loadType.type = json.GetType().Name;

        return ToBytes("[Packet]" + JsonConvert.SerializeObject(loadType));
    }

	// Converts an int to bytes
    public static byte[] ToBytes(int i)
	{
		return Encoding.UTF8.GetBytes(i.ToString());
	}

	// Converts a string to bytes
	public static byte[] ToBytes(string i)
	{
		return Encoding.UTF8.GetBytes(i);
	}

	// Small class to load packets in order to differentiate type
	[Serializable]
	class LoadType
	{
		public string type;
		public string parameters;
	}
}