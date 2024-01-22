using packets;
using game;

namespace card
{
    public class Green_Slime : BaseCard
    {
        public Green_Slime(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public override bool Place()
        {
            game.PlayerDamage(game.currentBoard.GetPlayer(action.placerId), 250);

            return base.Place();
        }
    }
}