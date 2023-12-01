using Newtonsoft.Json;
using System;
using System.Text;

public class PacketManager
{
	// Parse the incoming data into packets and run the packet-specific data
	public static void Decode(byte[] _data, Client client)
	{
		string[] dataList = Encoding.ASCII.GetString(_data).Split("[Packet]");

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
        return Encoding.ASCII.GetBytes(i.ToString());
    }

	// Convert a string to bytes
    public static byte[] ToBytes(string i)
    {
        return Encoding.ASCII.GetBytes(i);
    }

	// Class to discern the packet type
    [Serializable]
    class LoadType
    {
        public string type;
        public string parameters;
    }
}