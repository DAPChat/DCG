using packets;
using game;
using player;

namespace card
{
    public abstract class BaseCard
    {
        public CAP action;
        public Game game;

        public virtual bool Place()
        {
            if (action.card.Rank == "F" || action.card.Rank == "E" || action.card.Class == "Spell" || action.action == "summon")
            {
                game.PlaceCard(action);
                return true;
            }

            double sacrifice = 0;

            foreach (var card in action.param)
            {
                sacrifice += game.currentBoard.GetPlayer(action.placerId).fieldRowOne[card].SacrificialValue;
            }

            int match = 0;

            switch (action.card.Rank)
            {
                case "D":
                    match = 1;
                    break;
                case "C":
                    match = 2;
                    break;
                case "B":
                    match = 3;
                    break;
                case "A":
                    match = 4;
                    break;
                case "S":
                    match = 5;
                    break;
            }

            if (sacrifice < match) return false;
            else
            {
                for (int i = 0; i < action.param.Count; i++)
                {
                    game.RemoveCard(action, game.currentBoard.GetPlayer(action.placerId).fieldRowOne, action.placerId, action.param[i]);
                }

                action.param.Clear();

                game.PlaceCard(action);
            }

            return true;
        }

        public bool Summon()
        {
            Place();
            return false;
        }

        public virtual bool Death()
        {
            Player p = game.currentBoard.GetPlayer(game.OpponentId(action.placerId));

            CAP uCAP = new()
            {
                action = "fadd",
                card = Database.GetCard(p.fieldRowOne[action.targetSlot].Id).TempCard()
            };

            game.clients[p.id].tcp.WriteStream(PacketManager.ToJson(uCAP));

            p.forgotten.Add(p.fieldRowOne[action.targetSlot].Id);

            game.RemoveCard(action, p.fieldRowOne, p.id, action.targetSlot);

            game.currentBoard.UpdatePlayer(p);

            return true;
        }

        public virtual bool Update()
        {
            game.SendAll(PacketManager.ToJson(new CAP { action = "update", targetId = action.placerId, card = action.card, targetSlot = action.targetSlot }));

            return true;
        }

        public bool Attack()
        {
            game.Damage(action, this);

            return true;
        }
    }
}