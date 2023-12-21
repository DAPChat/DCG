using Godot;
using System;

namespace deck
{
    public partial class Editor : Control
    {
        static Button AddCards;
        static GridContainer TheArea;
        public override void _Ready()
        {
            TheArea = (GridContainer)GetNode("ScrollContainer/GridContainer");
            AddCards = (Button)GetNode("Add");
            AddCards.Pressed += () => { LoadCardsIn();AddCards.Hide(); };
        }
        public static void LoadCardsIn()
        {
            ClearGrid();
            //foreach (card in CurrentDeck)
            //ServerManager.client.WriteStream(PacketManager.ToJson(new CRP { cardId = card, main = true }));

            

        }
        public static void ClearGrid()
        {
            TheArea.GetChildren().Clear();
        }
    }
}