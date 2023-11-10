using Newtonsoft.Json;
using System;
using System.Text;

public class PacketManager
{
	public void Decode(byte[] _data, Client client)
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

	public static byte[] ToJson (object json)
	{
		LoadType loadType = new LoadType();

		loadType.parameters = JsonConvert.SerializeObject(json);
		loadType.type = json.GetType().Name;

		return ToBytes(JsonConvert.SerializeObject(loadType));
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