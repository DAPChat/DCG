namespace packets
{
    public class CAP : Packet
    {
        public int placerId;

        public GameScene.CardObject card;
        public string action;
        public int targetSlot;
        public int senderSlot;
    }
}