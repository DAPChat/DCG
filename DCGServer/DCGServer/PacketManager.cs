using Newtonsoft.Json;
using System.Text;

using packets;

public class PacketManager
{
	// Parse the incoming data into packets and run the packet-specific data
	public static void Decode(byte[] _data, Client client)
	{
		string[] dataList = Encoding.UTF8.GetString(_data).Split("[Packet]");

		for (int i = 1; i < dataList.Length; i++)
		{
			string data = dataList[i];

			try
			{
				var lt = JsonConvert.DeserializeObject<LoadType>(data.ToString());

				switch (lt.type)
				{
					case ("CSP"):
						var c = JsonConvert.DeserializeObject<CSP>(lt.parameters);

						c.Run(client);

						break;

					case "ACP":
						var login = JsonConvert.DeserializeObject<ACP>(lt.parameters);

						login.Run(client);
						
						break;
					case "CRP":
						var crp = JsonConvert.DeserializeObject<CRP>(lt.parameters);
						
						crp.Run(client);

						break;

					case "CAP":
						var cap = JsonConvert.DeserializeObject<CAP>(lt.parameters);

						Server.games[client.gameId].RegisterAction(cap);

						break;

					case "GSP":
						var gsp = JsonConvert.DeserializeObject<GSP>(lt.parameters);

						Server.games[client.gameId].currentBoard.NextPhase();
						
						break;
                }
			}
			catch (Exception e)
			{
				Console.WriteLine(client.id);

				Console.WriteLine(data.ToString());

				Console.WriteLine($"{e}");
			}
		}

		}

	// Convert a class to the properly formatted Json
	public static byte[] ToJson (object json)
	{
		LoadType loadType = new LoadType();

		loadType.parameters = JsonConvert.SerializeObject(json);
		loadType.type = json.GetType().Name;

		return ToBytes("[Packet]" + JsonConvert.SerializeObject(loadType));
	}

	// Convert an int to bytes
    public static byte[] ToBytes(int i)
    {
        return Encoding.UTF8.GetBytes(i.ToString());
    }

	// Convert a string to bytes
    public static byte[] ToBytes(string i)
    {
        return Encoding.UTF8.GetBytes(i);
    }

	// Class to discern the packet type
    [Serializable]
    class LoadType
    {
        public string type;
        public string parameters;
    }
}