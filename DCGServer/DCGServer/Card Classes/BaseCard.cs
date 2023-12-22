using packets;
using game;

namespace card
{
    public abstract class BaseCard
    {
        public CAP action;
        public Game game;

        public abstract void Run();

        public void Attack()
        {
            game.Damage(action);
        }
    }
}