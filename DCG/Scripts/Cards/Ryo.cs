using packets;
using static GameScene;

namespace card
{
    public class Ryo : BaseCard
    {
        public class Remembrance : Action
        {
            public override string name => "Remembrance";
            public override void Run(Card card)
            {
                card.AddEffect("Attack", 1);
                card.AddEffect(name, -1);

                targetPlace = this;

                ShowForgotten();
                ChooseView();
            }

            public override void Run(Card card, int slot)
            {
                targetPlace = null;
                ServerManager.Print("Here");

                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = cardObject, action = "summon", targetSlot = slot }));
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, action = GetType().Name }));

                ReturnView(card);
            }
        }

        public class Solitude : Action
        {
            public override string name => "Solitude";
            public override void Run(Card card)
            {
                
            }

            public override void Run(Card card, int slot)
            {

            }
        }

        
    }
}