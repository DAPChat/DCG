namespace packets
{
    public class CRP : Packet
    {
        public string cardId;
        public GameScene.CardObject card;
        public bool main;
    }
}