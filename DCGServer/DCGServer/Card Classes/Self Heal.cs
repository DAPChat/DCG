using packets;
using game;
using player;

namespace card
{
    public class Self_Heal : BaseCard
    {
        public Self_Heal(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public bool Heal()
        {
            Player p = game.currentBoard.GetPlayer(action.placerId);

            p.lifePoints += 500;

            game.currentBoard.UpdatePlayer(p);

            game.RemoveCard(action, p.fieldRowTwo, p.id, action.senderSlot);

            game.SendAll(PacketManager.ToJson(new PUP { action = "uhp", player = p.Client() }));

            return false;
        }
    }
}