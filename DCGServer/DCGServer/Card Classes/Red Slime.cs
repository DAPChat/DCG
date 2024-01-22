using packets;
using game;

namespace card
{
    public class Red_Slime : BaseCard
    {
        public Red_Slime(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public override bool Death()
        {
            game.PlayerDamage(game.currentBoard.GetPlayer(action.placerId), -250);

            return base.Death();
        }
    }
}