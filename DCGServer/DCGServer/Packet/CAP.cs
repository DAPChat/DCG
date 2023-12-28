using card;

namespace packets
{
    public class CAP : Packet
    {
        public int placerId;

        public TempCard card;
        public string action;
        public int targetSlot;
        public int senderSlot;

        public CAP Clone()
        {
            CAP cap = new()
            {
                placerId = placerId,
                card = card,
                action = action,
                targetSlot = targetSlot
            };

            return cap;
        }
    }
}