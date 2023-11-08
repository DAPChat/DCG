using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public partial class Card : Node3D
{
    //when using this you will do var newcard = new Card(dictionary,cardnum)

    public Card(Dictionary<dynamic, dynamic> cards, int req)
    {
        var cardName = GetNode<Label>("FrontFace/Name/Name");
        cardName.Text = cards[req]["Name"];
        var cardRank = GetNode<Label>("FrontFace/Name/Rank");
        cardRank.Text = cards[req]["Rank"];
        var cardDescription = GetNode<Label>("FrontFace/Description/Description");
        cardDescription.Text = cards[req]["Description"];
        var cardStats = GetNode<Label>("FrontFace/Description/Stats");
        cardStats.Text = cards[req]["ATK"] + " ATK / " + cards[req]["HP"] + " HP";
        var newImage = ImageTexture.CreateFromImage(cards[req]["Image"]);
        var mesh = GetNode<MeshInstance3D>("FrontFace/Picture");
        var material = mesh.GetActiveMaterial(0) as StandardMaterial3D;
        material!.AlbedoTexture = newImage;
        //add switch case for the type of card
        //in switch case change background of card
    }
}