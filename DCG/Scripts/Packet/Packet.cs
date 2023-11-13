using System;

[Serializable]
public class Packet
{
    public int senderId;
    public int targetId;

    public virtual void Run(Client client)
    {

    }
}