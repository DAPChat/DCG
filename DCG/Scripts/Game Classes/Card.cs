using Godot;
using System;

public partial class Card : Node3D { 
    private void getCard(string[] cards,int req)
    {
        var card = new Card();
        Label3D cardName = (Label3D)NodePath("FrontFace/Name/Name");
        cardName.Text = cards[req];
    }
}

