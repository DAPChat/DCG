using player;

namespace card
{
    public class Attack : BaseAction
    {
        public Attack(List<int> _actions)
        {
            args = _actions;
        }

        public override void Run(Player _t, Player _s)
        {
            
        }
    }
}