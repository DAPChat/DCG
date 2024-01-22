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
            if (action.card.Rank == "F" || action.card.Rank == "E" || action.card.Class == "Spell")
            {
                if (action.card.Class != "Spell")
                {
                    if (game.currentBoard.summoned >= 2) return false;
                    
                    game.currentBoard.summoned++;
                }

                game.PlaceCard(action);
                return false;
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

            return false;
        }

        public virtual bool Summon()
        {
            game.PlaceCard(action);
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
            game.AddStatus(this, "Attack", 1);

            Player p2 = game.currentBoard.GetPlayer(game.OpponentId(action.placerId));
            Player p1 = game.currentBoard.GetPlayer(action.placerId);

            bool empty = true;

            foreach (var card in p2.fieldRowOne)
            {
                if (card != null)
                {
                    empty = false;
                    break;
                }
            }

            if (empty)
            {
                game.PlayerDamage(action, p2, -action.card.Atk);

                game.currentBoard.UpdatePlayer(p2);

                return false;
            }

            CAP cap = action.Clone();
            cap.card = p2.fieldRowOne[action.targetSlot];

            var bs = ActionManager.CreateCard(cap, game);

            game.Damage(action);

            bool died = false;

            if (p2.fieldRowOne[action.targetSlot].Hp <= 0)
            {
                died = true;

                game.PlayerDamage(action, p2, p2.fieldRowOne[action.targetSlot].Hp);

                bs.Death();
            }

            if (!died)
            {
                var atkCard = p1.fieldRowOne[action.senderSlot];
                var defCard = p2.fieldRowOne[action.targetSlot];

                if (defCard.Atk > atkCard.Atk)
                {
                    CAP ac = new() { card = atkCard, targetSlot = action.senderSlot, placerId = p2.id };
                    var cardBase = ActionManager.CreateCard(ac, game);

                    game.Damage(ac, defCard.Atk - atkCard.Atk);

                    if (p1.fieldRowOne[action.senderSlot].Hp <= 0)
                    {
                        cardBase.Death();
                    }
                }
            }

            return false;
        }
    }
}