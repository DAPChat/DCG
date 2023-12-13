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
            AddCards = (Button)GetNode("Add");
            AddCards.Pressed += () => LoadCardsIn();
        }
        public void LoadCardsIn()
        {
            ClearGrid();

            //TheArea
        }
        public void ClearGrid()
        {

        }
    }
}