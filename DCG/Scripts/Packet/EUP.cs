namespace packets
{
    public class EUP : Packet
    {
        public string type;
        public string name;
        public int param;
        public int slot;

        public GameScene.CardObject card;
    }
}