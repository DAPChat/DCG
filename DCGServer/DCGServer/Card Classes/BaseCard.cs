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
            game.currentBoard.GetPlayer(action.senderSlot).forgotten.Add(action.card.Id);
            game.currentBoard.GetPlayer(action.senderSlot).fieldRowOne.SetValue(null, action.targetSlot);

            CAP uCAP = new()
            {
                action = "fadd",
                card = action.card
            };

            game.clients[action.senderSlot].tcp.WriteStream(PacketManager.ToJson(uCAP));
        }

        public void Attack()
        {
            game.Damage(action, this);
        }
    }
}