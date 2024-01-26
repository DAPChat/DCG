using packets;
using game;

namespace card
{
    public class White_Slime : BaseCard
    {
        public White_Slime(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public bool Buff()
        {
            game.AddEffect(action, "Atk", 1, 600);

            return false;
        }
    }
}