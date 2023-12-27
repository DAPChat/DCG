using packets;
using game;

namespace card
{
    public abstract class BaseCard
    {
        public CAP action;
        public Game game;

        public virtual void Summon(Game game)
        {

        }

        public virtual void Death(Game game, CAP action)
        {
            game.currentBoard.GetPlayer(action.targetId).forgotten.Add(action.card.Id);
            game.currentBoard.GetPlayer(action.targetId).fieldRowOne.SetValue(null, action.slot);

            CAP uCAP = new()
            {
                action = "fadd",
                card = action.card
            };

            game.clients[action.targetId].tcp.WriteStream(PacketManager.ToJson(uCAP));
        }

        public void Attack()
        {
            game.Damage(action, this);
        }
    }
}