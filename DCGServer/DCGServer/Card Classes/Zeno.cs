﻿using packets;
using game;

namespace card
{
    public class Zeno : BaseCard
    {
        public Zeno(CAP _action, Game _game)
        {
            action = _action;
            game = _game;
        }

        public override void Update()
        {
            game.ResetStats(action.placerId, action.targetSlot, new string[] { "Hp", "Mana" });

            int count = 0;

            foreach (var r in game.currentBoard.GetPlayer(game.OpponentId(action.placerId)).fieldRowOne)
                if (r != null)
                    count += 1;

            action.card.Atk += count * 250;

            game.currentBoard.GetPlayer(action.placerId).fieldRowOne[action.targetSlot].Atk = action.card.Atk;

            game.SendAll(PacketManager.ToJson(new CAP { action = "update", targetId = action.placerId, card = action.card, targetSlot = action.targetSlot }));

            base.Update();
        }
    }
}