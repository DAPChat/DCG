using packets;
using game;

namespace card
{
    public class Ryo : BaseCard
    {
        public Ryo (CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public bool Remembrance()
        {
            if (!game.Cost(this, "Mana", 700)) return false;

            if (game.currentBoard.GetPlayer(action.placerId).forgotten.Contains(action.paramCard.Id))
            {
                CAP ac = action.Clone();
                ac.card = action.paramCard;
                ac.paramCard = null;

                game.PlaceCard(ac);
                game.currentBoard.GetPlayer(action.placerId).forgotten.Remove(action.paramCard.Id);
            }
            else return false;

            game.AddStatus(this, "Attack", 2);
            game.AddStatus(this, action.action, -1);

            return false;
        }

        public bool Solitude()
        {
            if(!game.Cost(this, "Mana", 400)) return false;

            game.AddEffect(action, "Immortal", 1, 0);
            game.AddStatus(this, action.action, 2);
            game.AddStatus(this, "Attack", 1);

            return false;
        }

        public bool HB()
        {
            if (!game.Cost(this, "Mana", 300)) return false;

            game.AddEffect(action, "Hp", 2, 750);
            game.AddEffect(action, "Atk", 2, 750);

            return false;
        }

        public override bool Death()
        {
            base.UnDeath(2);
            Unforgotten();
            return true;
        }

        public override void Unforgotten()
        {
            TempCard[] field = game.currentBoard.GetPlayer(game.OpponentId(action.placerId)).fieldRowOne;

            for (int i = 0; i < field.Length; i++)
            {
                if (field[i] == null) continue;

                CAP ac = new()
                {
                    senderSlot = i,
                    placerId = game.OpponentId(action.placerId)
                };

                game.AddEffect(ac, "Atk", 1, 500);
            }
        }
    }
}