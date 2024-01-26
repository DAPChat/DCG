using packets;
using static GameScene;

namespace card
{
    public class White_Slime : BaseCard
    {
        public class Buff : Action
        {
            public override string name => "Attack Power";

            public override void Run(Card card)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, action = GetType().Name }));
            }

            public override void Run(Card card, int slot)
            {
                
            }
        }
    }
}