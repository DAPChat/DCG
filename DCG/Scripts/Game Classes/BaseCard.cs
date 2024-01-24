using packets;

namespace card
{
    public abstract class BaseCard
    {
        public bool skip = false;

        public class Attack : Action
        {
            public override string name => "Attack";
            public override void Run(Card card)
            {
                bool empty = true;

                foreach (var item in GameScene.cards)
                {
                    if (item.placerId != ServerManager.client.id && item.card.Class != "Spell")
                    {
                        empty = false;
                        break;
                    }
                }

                if (empty)
                {
                    ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, action = GetType().Name }));
                    return;
                }

                GameScene.ChooseView();
                GameScene.chooseTarget = this;
            }

            public override void Run(Card card, int slot)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, action = GetType().Name, targetSlot = slot }));
            }
        }

        public virtual void Summon(GameScene.CardObject card, int slot)
        {
            double match = 0;

            switch (card.Rank)
            {
                case "D":
                    match = 1;
                    break;
                case "C":
                    match = 1.5;
                    break;
                case "B":
                    match = 2;
                    break;
                case "A":
                    match = 3;
                    break;
                case "S":
                    match = 4;
                    break;
            }

            GameScene.matchSacrifice = match;
            GameScene.sacrificialAmt = 0;
            GameScene.sacrificed.Clear();
            GameScene.toSummon.Add(card, slot);
            GameScene.ChooseSacrifice(card, slot);
        }
    }
}