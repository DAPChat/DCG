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
					case ("CSP"):
						var c = JsonConvert.DeserializeObject<CSP>(lt.parameters);

						client.values["Connection"] = c.Run().ToString();

						break;

					case ("GSP"):
						var g = JsonConvert.DeserializeObject<GSP>(lt.parameters);

						client.values["Game Id"] =  g.gameId.ToString();

						break;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"{e.Message}");
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