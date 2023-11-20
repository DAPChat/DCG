public class ACP : Packet
{
    public bool create;

    public string username;
    public string password;

    public ACP(bool _create, string _username, string _password)
    {
        create = _create;
        username = _username;
        password = _password;
    }

    public override void Run(Client client)
    {
        if (create)
        {
            if (Database.CheckAvailableAcc(username))
            {
                Database.AddAcc(username, password);

                create = true;
                client.tcp.WriteStream(PacketManager.ToJson(this));
            }
            else
            {
                create = false;
                client.tcp.WriteStream(PacketManager.ToJson(this));
            }
        }
        else
        {
            if(Database.VerifyAcc(username, password))
            {
                create = true;
                client.tcp.WriteStream(PacketManager.ToJson(this));
            }
            else
            {
                create = false;
                client.tcp.WriteStream(PacketManager.ToJson(this));
            }
        }
    }
}