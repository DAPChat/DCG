using packets;
using game;

namespace card
{
    public class Goblin : BaseCard
    {
        public Goblin(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }
    }
}