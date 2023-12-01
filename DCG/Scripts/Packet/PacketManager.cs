using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

public class PacketManager
{
	public static void Decode(byte[] _data, Client client)
	{
		string[] dataList = Encoding.ASCII.GetString(_data).Split("[Packet]");

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
						var gsp = JsonConvert.DeserializeObject<GSP>(lt.parameters);

						if (client.gameId != gsp.gameId)
						{
							client.gameId = gsp.gameId;
							Main.inGame = !Main.inGame ? true : false;

							if (!Main.inGame)
							{
								GameScene.changeScene = true;
							}
                        }

						break;

					case "Player":
						var player = JsonConvert.DeserializeObject<Player>(lt.parameters);

						ServerManager.client.player = player;

						ServerManager.Print(player.playerNum.ToString());
						ServerManager.Print(player.deck.Count.ToString());

						break;

					case "PlayerAccount":
						var account = JsonConvert.DeserializeObject<PlayerAccount>(lt.parameters);

						client.account = account;

						break;

					case "Card":
						var card = JsonConvert.DeserializeObject<GameScene.CardObject>(lt.parameters);

						GameScene.cardObject = card;

						break;

					case "CAP":
						var cap = JsonConvert.DeserializeObject<CAP>(lt.parameters);

						switch (cap.action)
						{
							case "place":
								GameScene.cardObject = cap.card;
								// GameScene.PlaceCard(cap);
								break;
						}

						break;

					case "ACP":
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
				}
			}
			catch (Exception e)
			{
				ServerManager.Print($"{e.Message}");
			}
		}
	}

    public static byte[] ToJson(object json)
    {
        LoadType loadType = new LoadType();

        loadType.parameters = JsonConvert.SerializeObject(json);
        loadType.type = json.GetType().Name;

        return ToBytes("[Packet]" + JsonConvert.SerializeObject(loadType));
    }

    public static byte[] ToBytes(int i)
	{
		return Encoding.ASCII.GetBytes(i.ToString());
	}

	public static byte[] ToBytes(string i)
	{
		return Encoding.ASCII.GetBytes(i);
	}

	[Serializable]
	class LoadType
	{
		public string type;
		public string parameters;
	}
}