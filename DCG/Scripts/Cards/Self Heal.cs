using packets;

namespace card
{
    public class Self_Heal : BaseSpell
    {
        public class Heal : Action
        {
            public override string name => "Heal";

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