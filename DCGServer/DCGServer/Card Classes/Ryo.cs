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
            if (!game.Cost(this, "Mana", 700)) return;

            game.AddStatus(this, "Attack", 2);
            game.AddStatus(this, action.action, -1);
        }

        public void Solitude()
        {
            if(!game.Cost(this, "Mana", 400)) return;

            game.AddEffect(this, "Immortal", 1, 0);
            game.AddStatus(this, action.action, 2);
            game.AddStatus(this, "Attack", 1);
        }

        public void HB()
        {
            if (!game.Cost(this, "Mana", 300)) return;

            game.AddEffect(this, "Hp", 2, 750);
            game.AddEffect(this, "Atk", 2, 750);
        }

        public override void Death()
        {
            base.Death();
        }
    }
}