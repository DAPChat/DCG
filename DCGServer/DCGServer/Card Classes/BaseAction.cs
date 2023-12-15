using player;

namespace card
{
    public abstract class BaseAction
    {
        public List<int> args;

        public Player target;
        public Player sender;

        public abstract void Run(Player _target, Player _sender);
    }
}