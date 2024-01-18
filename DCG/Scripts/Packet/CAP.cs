using System.Collections.Generic;

namespace packets
{
    public class CAP : Packet
    {
        public int placerId;

        public GameScene.CardObject card;
        public GameScene.CardObject paramCard;
        public string action;
        public int targetSlot;
        public int senderSlot;

        public List<int> param;
    }
}