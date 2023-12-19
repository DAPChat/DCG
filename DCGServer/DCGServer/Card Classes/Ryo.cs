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

        public void Attack()
        {
            game.Damage(action);
        }

        public override void Run()
        {
            Console.WriteLine("Hah");
        }
    }
}