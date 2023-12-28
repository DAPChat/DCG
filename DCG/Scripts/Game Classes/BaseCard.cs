using packets;

namespace card
{
    public abstract class BaseCard
    {
        public class Attack : Action
        {
            public override string name => "Attack";
            public override void Run(Card card)
            {
                GameScene.ChooseView();
                GameScene.choose = this;
            }

            public override void Run(Card card, int slot)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, action = GetType().Name, targetSlot = slot }));
            }
        }
    }
}