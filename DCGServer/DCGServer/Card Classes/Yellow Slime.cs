using packets;
using game;

namespace card
{
    public class Yellow_Slime : BaseCard
    {
        public Yellow_Slime(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public override bool Summon()
        {
            foreach (var c in action.handParam)
            {
                game.clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(new CAP { card = Database.GetCard(c).TempCard(), action = "hremove" }));
                game.currentBoard.GetPlayer(action.placerId).hand.Remove(c);
            }

            game.clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(new CAP { card = action.card, action = "hremove" }));
            game.currentBoard.GetPlayer(action.placerId).hand.Remove(action.card.Id);

            return base.Summon();
        }
    }
}