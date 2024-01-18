using packets;
using game;

namespace card
{
    public class Blue_Slime : BaseCard
    {
        public Blue_Slime(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public override bool Summon()
        {
            game.clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(new CAP { card = action.card, action = "hremove" }));
            game.currentBoard.GetPlayer(action.placerId).hand.Remove(action.card.Id);

            return base.Summon();
        }
    }
}