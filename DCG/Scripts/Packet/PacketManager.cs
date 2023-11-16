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
						var csp= JsonConvert.DeserializeObject<CSP>(lt.parameters);

						client.values["Connection"] = csp.Run().ToString();

						break;

					case "GSP":
						var gsp = JsonConvert.DeserializeObject<GSP>(lt.parameters);

						client.gameId = gsp.gameId;

						client.values["Game Id"] =  gsp.gameId.ToString();

						break;

					case "Player":
						var player = JsonConvert.DeserializeObject<Player>(lt.parameters);

						client.values["Player Id"] = player.id.ToString();

						break;

					case "Card":
						var card = JsonConvert.DeserializeObject<Main.CardObject>(lt.parameters);

						Main.cardObject = card;

						break;
				}
			}
			catch (Exception e)
			{
				ServerManager.Print($"{e.Message}");
			}
		}
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