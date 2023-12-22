using packets;

namespace card
{
    public class Ryo : BaseCard
    {
        public class Remembrance : Action
        {
            public override string name => "Remembrance";
            public override void Run(GameScene.CardObject card)
            {

            }

            public override void Run(GameScene.CardObject card, int slot)
            {

            }
        }

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

        public class Solitude : Action
        {
            public override string name => "Solitude";
            public override void Run(GameScene.CardObject card)
            {
                
            }

            public override void Run(GameScene.CardObject card, int slot)
            {

            }
        }

        public override void Summon()
        {
            ServerManager.Print("Here");
        }

        public override void Death()
        {
            
        }
    }
}