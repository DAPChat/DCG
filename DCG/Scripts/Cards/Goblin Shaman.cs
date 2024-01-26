using packets;
using static GameScene;

namespace card
{
    public class Goblin_Shaman : BaseCard
    {
        public class Assist : Action
        {
            public override string name => "Goblin Assist";

            public override void Run(Card card)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, action = GetType().Name }));
            }

            public override void Run(Card card, int slot)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}