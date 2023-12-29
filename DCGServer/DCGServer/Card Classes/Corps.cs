using packets;
using game;

namespace card
{
    public class Corps : BaseCard
    {
        public Corps(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }
    }
}