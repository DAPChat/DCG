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
                GameScene.chooseTarget = this;
            }

            public override void Run(Card card, int slot)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, senderSlot = card.slot, card = card.card, action = GetType().Name, targetSlot = slot }));
            }
        }

        public virtual void Summon(GameScene.CardObject card, int slot)
        {
            if (card.Type == "Spell") ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = card, action = "place", targetSlot = slot }));

            int match = 0;

            switch (card.Rank)
            {
                case "D":
                    match = 1;
                    break;
                case "C":
                    match = 2;
                    break;
                case "B":
                    match = 3;
                    break;
                case "A":
                    match = 4;
                    break;
                case "S":
                    match = 5;
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