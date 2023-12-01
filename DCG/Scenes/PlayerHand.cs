using Godot;
using System;

public partial class PlayerHand : Container
{
	public void addCard(string[] cards)
    {
		foreach (var card in cards)
		{
            //get cards from database
            // getCardFromDataBase(card)
            //var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/2d_card.tscn").Instantiate().Duplicate();
            //GetNode()
        }
    }
}
