﻿using packets;
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
                card.AddStatus("Attack", 2);
                card.AddStatus(name, -1);

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