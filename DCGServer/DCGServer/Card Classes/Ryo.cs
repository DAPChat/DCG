using packets;
using game;

namespace card
{
    public class Ryo : BaseCard
    {
        public Ryo (CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public void Remembrance()
        {
            game.AddEffect(this, "Attack", 1);
            game.AddEffect(this, action.action, -1);
        }

        public void Solitude()
        {

        }

        public override void Death()
        {
            base.Death();
        }
    }
}