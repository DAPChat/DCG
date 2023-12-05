public class CRP : Packet
{
    public string cardId;
    public Card card;
    public override void Run(Client client)
    {
        if (cardId != null)
        {
            client.tcp.WriteStream(PacketManager.ToJson(new CRP { card = Database.GetCard(cardId) }));
        }
    }
}