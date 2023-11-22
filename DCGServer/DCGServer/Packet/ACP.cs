public class ACP : Packet
{
    public bool create;

    public string username;
    public string password;

    public string error;

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
                PlayerAccount account = Database.AddAcc(this, client);

                create = true;
                client.tcp.WriteStream(PacketManager.ToJson(this));
                client.tcp.WriteStream(PacketManager.ToJson(account));
            }
            else
            {
                create = false;
                error = "Username is already in use";
                client.tcp.WriteStream(PacketManager.ToJson(this));
                return;
            }
        }
        else
        {
            PlayerAccount account = Database.VerifyAcc(this, client);

            if (account != null)
            {
                create = true;
                client.tcp.WriteStream(PacketManager.ToJson(this));
                client.tcp.WriteStream(PacketManager.ToJson(account));
            }
            else
            {
                create = false;
                client.tcp.WriteStream(PacketManager.ToJson(this));
                return;
            }
        }
    }
}