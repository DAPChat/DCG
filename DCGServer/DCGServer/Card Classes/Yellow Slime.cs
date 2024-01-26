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
            player.Player p = game.currentBoard.GetPlayer(action.placerId);

            foreach (var c in action.handParam)
            {
                TempCard card = Database.GetCard(c).TempCard();

                game.clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(new CAP { card = card, action = "hremove" }));
                p.hand.Remove(c);

                CAP uCAP = new()
                {
                    action = "fadd",
                    card = card
                };

                game.clients[p.id].tcp.WriteStream(PacketManager.ToJson(uCAP));

                p.forgotten.Add(card.Id);

                game.currentBoard.UpdatePlayer(p);
            }

            game.clients[action.placerId].tcp.WriteStream(PacketManager.ToJson(new CAP { card = action.card, action = "hremove" }));
            game.currentBoard.GetPlayer(action.placerId).hand.Remove(action.card.Id);

            return base.Summon();
        }
    }
}