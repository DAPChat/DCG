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
                if (forgotten.Count == 0) return;

                targetPlace = this;

                ShowForgotten();
                ChooseView();
            }

            public override void Run(Card card, int slot)
            {
                targetPlace = null;

                CardObject rCard = null;

                foreach (var c in forgotten)
                {
                    if (c.Id == cardObject.Id)
                    {
                        rCard = c;
                        break;
                    }
                }

                if (rCard != null) forgotten.Remove(rCard);

                //ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = cardObject, action = "summon", targetSlot = slot }));
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, paramCard = cardObject, targetSlot = slot, action = GetType().Name }));

                ReturnView(card);
            }
        }

        public class Solitude : Action
        {
            public override string name => "Solitude";
            public override void Run(Card card)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = card.card, senderSlot = card.slot, action = GetType().Name}));
            }

            public override void Run(Card card, int slot)
            {

            }
        }

        public class HB : Action
        {
            public override string name => "Heat of the Battle";
            public override void Run(Card card)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = card.card, senderSlot = card.slot, action = GetType().Name }));
            }

            public override void Run(Card card, int slot)
            {

            }
        }
    }
}