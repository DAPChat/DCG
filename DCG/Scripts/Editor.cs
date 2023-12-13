using Godot;
using System;

public partial class Editor : Control
{
	static Button AddCards;
    static GridContainer TheArea;
	public override void _Ready()
	{
        AddCards = (Button)GetNode("Home/UI/Editor/Add");
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
