using packets;

namespace card
{
    public class Blue_Slime : BaseCard
    {
        public override void Summon(GameScene.CardObject card, int slot)
        {
            bool summon = true;

            foreach (var item in GameScene.cards)
            {
                if (item.placerId == ServerManager.client.id)
                {
                    summon = false;
                    break;
                }
            }

            if (summon)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = card, action = "summon", targetSlot = slot }));
            }
            else
            {
                base.Summon(card, slot);
            }
        }
    }
}