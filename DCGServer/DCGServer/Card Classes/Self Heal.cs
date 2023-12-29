using packets;
using game;

namespace card
{
    public class Self_Heal : BaseCard
    {
        public Self_Heal(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }
    }
}