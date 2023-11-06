using System;
using System.Text;
using System.Text.Json;

public class Packet
{
    public static void Decode(byte[] _data)
    {
        string data = Encoding.ASCII.GetString(_data);

        if (data == null) return;

        LoadType lt = JsonSerializer.Deserialize<LoadType>(data);

        lt.Back();

        if (lt.type == "Connection")
        {
            Connection c = JsonSerializer.Deserialize<Connection>(lt.parameters);

            Console.WriteLine(c.CheckPing());
        }
    }

    class LoadType
    {
        public string type;
        public string parameters;

        public void Back()
        {
            Console.WriteLine(type + " : " + parameters.ToString());
        }
    }
}