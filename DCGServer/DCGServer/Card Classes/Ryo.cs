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

        public bool Remembrance()
        {
            if (!game.Cost(this, "Mana", 700)) return false;

            game.AddStatus(this, "Attack", 2);
            game.AddStatus(this, action.action, -1);

            return true;
        }

        public bool Solitude()
        {
            if(!game.Cost(this, "Mana", 400)) return false;

            game.AddEffect(this, "Immortal", 1, 0);
            game.AddStatus(this, action.action, 2);
            game.AddStatus(this, "Attack", 1);

            return true;
        }

        public bool HB()
        {
            if (!game.Cost(this, "Mana", 300)) return false;

            game.AddEffect(this, "Hp", 2, 750);
            game.AddEffect(this, "Atk", 2, 750);

            return true;
        }

        public override bool Death()
        {
            return base.Death();
        }
    }
}