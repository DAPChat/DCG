using Newtonsoft.Json;
using System;
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

						client.gameId = gsp.gameId;

						break;

					case "Player":
						var player = JsonConvert.DeserializeObject<Player>(lt.parameters);

						break;

					case "Card":
						var card = JsonConvert.DeserializeObject<GameScene.CardObject>(lt.parameters);

						GameScene.cardObject = card;

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