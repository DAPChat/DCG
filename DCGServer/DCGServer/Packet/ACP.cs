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
            // Run all the checks in order to verify the account availability
            if (Database.CheckAvailableAcc(username))
            {
                // Add the account if valid
                PlayerAccount account = Database.AddAcc(this, client);

                // Send confirmation and account data to the client
                create = true;
                client.tcp.WriteStream(PacketManager.ToJson(this));
                client.tcp.WriteStream(PacketManager.ToJson(account));
            }
            else
            {
                // Send error if unavailable or invalid
                create = false;
                error = "Username is already in use";
                client.tcp.WriteStream(PacketManager.ToJson(this));
                return;
            }
        }
        else
        {
            // Verify if the account password is correct
            PlayerAccount account = Database.VerifyAcc(this, client);

            if (account != null)
            {
                // Send confirmation and account data to the client
                create = true;
                client.tcp.WriteStream(PacketManager.ToJson(this));
                client.tcp.WriteStream(PacketManager.ToJson(account));
            }
            else
            {
                // Send error if incorrect information was given
                create = false;
                client.tcp.WriteStream(PacketManager.ToJson(this));
                return;
            }
        }
    }
}