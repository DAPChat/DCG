using packets;
using game;

namespace card
{
    public class Napoleon_Bonaparte : BaseCard
    {
        public Napoleon_Bonaparte(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public override bool Update()
        {
            game.ResetStats(action.placerId, action.targetSlot, new string[] { "Atk" });
            player.Player p = game.currentBoard.GetPlayer(action.placerId);

            for (int i = 0; i < p.fieldRowOne.Length; i++)
            {
                if (p.fieldRowOne[i] == null) continue;
                if (p.fieldRowOne[i].Name == "Corps")
                {
                    p.fieldRowOne[action.targetSlot].Atk += 1000;
                    break;
                }
            }

            return base.Update();
        }
    }
}