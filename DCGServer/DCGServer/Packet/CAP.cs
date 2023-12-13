using card;

namespace packets
{
    public class CAP : Packet
    {
        public int placerId;

        public TempCard card;
        public string action;
        public int slot;

        public CAP Clone()
        {
            CAP cap = new()
            {
                placerId = placerId,
                card = card,
                action = action,
                slot = slot
            };

            return cap;
        }
    }
}