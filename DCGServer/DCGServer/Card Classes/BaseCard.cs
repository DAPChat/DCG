using packets;
using game;
using player;

namespace card
{
    public abstract class BaseCard
    {
        public CAP action;
        public Game game;

        public virtual void Summon()
        {

        }

        public virtual void Death()
        {
            Player p = game.currentBoard.GetPlayer(game.OpponentId(action.placerId));

            CAP uCAP = new()
            {
                action = "fadd",
                card = Database.GetCard(p.fieldRowOne[action.targetSlot].Id).TempCard()
            };

            game.clients[p.id].tcp.WriteStream(PacketManager.ToJson(uCAP));

            p.forgotten.Add(p.fieldRowOne[action.targetSlot].Id);
            p.fieldRowOne.SetValue(null, action.targetSlot);

            game.currentBoard.UpdatePlayer(p);
        }

        public void Attack()
        {
            Player p = game.currentBoard.GetPlayer(action.targetId);
            // var card = Activator.CreateInstance(Type.GetType("card." + p.fieldRowOne[action.targetSlot].Name.Replace(' ', '_')), new object[] { action, game });
            // uncomment when we get classes for each card


            game.Damage(action, this);
        }
    }
}