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

            p.forgotten.Add(action.card.Id);
            p.fieldRowOne.SetValue(null, action.targetSlot);

            game.currentBoard.UpdatePlayer(p);

            CAP uCAP = new()
            {
                action = "fadd",
                card = action.card
            };

            game.clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(uCAP));
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