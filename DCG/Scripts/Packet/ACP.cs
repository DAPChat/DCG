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
}