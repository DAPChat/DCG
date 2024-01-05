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
            game.AddStatus(this, "Attack", 2);
            game.AddStatus(this, action.action, -1);
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