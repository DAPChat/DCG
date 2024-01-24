using Godot;
using packets;
using System.Collections.Generic;

namespace card
{
    public class Yellow_Slime : BaseCard
    {
        int needed = 2;
        
        public Yellow_Slime()
        {
            skip = false;
        }

        public override void Summon(GameScene.CardObject card, int slot)
        {
            if (!skip)
            {
                // if (GameScene.summonCard != this) return;

                if (GameScene.chosen.Count == needed)
                {
                    List<string> cards = new();

                    foreach (var c in GameScene.chosen)
                    {
                        cards.Add(c.Id);
                    }

                    GameScene.cardSummon.Clear();
                    GameScene.summonCard = null;
                    GameScene.chosen = new();
                    ((ScrollContainer)GameScene.sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Hide();
                    GameScene.HandShown = false;
                    ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = card, action = "summon", targetSlot = slot, handParam = cards }));
                    return;
                }

                GameScene.summonCard = this;
                GameScene.cardSummon[card] = slot;
                GameScene.GetViableCard("Slime", new string[] { "D", "C", "B", "A", "S" });

                return;
            }

            base.Summon(card, slot);
        }
    }
}