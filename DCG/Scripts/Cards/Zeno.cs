using packets;

namespace card
{
    public class Zeno : BaseCard
    {
        public class Attack : Action
        {
            public override string name => "Attack";
            public override void Run(GameScene.CardObject card)
            {
                GameScene.QuietReturn();
                GameScene.choose = this;
            }

            public override void Run(GameScene.CardObject card, int slot)
            {
                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = card, action = GetType().Name, slot = slot }));
            }
        }

        public override void Death()
        {
            throw new System.NotImplementedException();
        }

        public override void Summon()
        {
            throw new System.NotImplementedException();
        }
    }
}