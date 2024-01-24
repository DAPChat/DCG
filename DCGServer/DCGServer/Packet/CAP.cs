using card;

namespace packets
{
    public class CAP : Packet
    {
        public int placerId;

        public TempCard card;
        public TempCard paramCard;
        public string action;
        public int targetSlot;
        public int senderSlot;
        
        public List<int> param;
        public List<string> handParam;

        public CAP Clone()
        {
            CAP cap = new()
            {
                placerId = placerId,
                card = card,
                action = action,
                targetSlot = targetSlot,
                senderSlot = senderSlot,
                param = param,
                handParam = handParam
            };

            return cap;
        }
    }
}