using packets;
using game;
using player;

namespace card
{
    public class Self_Heal : BaseCard
    {
        public Self_Heal(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public bool Heal()
        {
            Player p = game.currentBoard.GetPlayer(action.placerId);

            p.lifePoints += 500;

            game.currentBoard.UpdatePlayer(p);

            CAP _action = action.Clone();
            _action.targetId = p.id;
            _action.targetSlot = _action.senderSlot;
            _action.card = p.fieldRowTwo[action.senderSlot].MakeReady();
            _action.action = "remove";

            game.SendAll(PacketManager.ToJson(new PUP { action = "uhp", player = p.Client() }));
            game.SendAll(PacketManager.ToJson(_action));

            p.fieldRowTwo.SetValue(null, action.senderSlot);

            return false;
        }
    }
}