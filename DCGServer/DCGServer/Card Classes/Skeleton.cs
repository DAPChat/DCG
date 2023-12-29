using packets;
using game;

namespace card
{
    public class Skeleton : BaseCard
    {
        public Skeleton(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }
    }
}