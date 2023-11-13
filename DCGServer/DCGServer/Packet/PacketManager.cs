using Newtonsoft.Json;
using System;
using System.Text;

public class PacketManager
{
    public void Decode(byte[] _data)
    {
        string data = Encoding.ASCII.GetString(_data);

        if (data == null) return;

        try
        {
            LoadType lt = JsonConvert.DeserializeObject<LoadType>(data);

            if (lt.type == "Connection")
            {
                Connection c = JsonConvert.DeserializeObject<Connection>(lt.parameters);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
        }
    }

    [Serializable]
    class LoadType
    {
        public string type;
        public string parameters;
    }
}