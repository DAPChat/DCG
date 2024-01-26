using packets;
using game;

namespace card
{
    public class Goblin_Shaman : BaseCard
    {
        public Goblin_Shaman(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public bool Assist()
        {
            player.Player p = game.currentBoard.GetPlayer(action.placerId);

            for (int i = 0; i < p.fieldRowOne.Length; i++)
            {
                if (p.fieldRowOne[i] == null) continue;
                if (i == action.senderSlot) continue;
                if (p.fieldRowOne[i].Type == "Goblin")
                {
                    CAP ac = new ()
                    {
                        placerId = action.placerId,
                        senderSlot = i
                    };

                    game.AddEffect(ac, "Atk", 1, 500);
                }
            }

            return false;
        }
    }
}