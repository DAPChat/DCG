using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public partial class Card : Node3D
{

    //when using this you will do var newcard = new Card(dictionary,cardnum)

    public Card(dynamic[] cards, int req)
    {
        var cardName = GetNode<Label>("FrontFace/Name/Name");
        cardName.Text = cards[req]["Name"];
        var cardRank = GetNode<Label>("FrontFace/Name/Rank");
        cardRank.Text = cards[req]["Rank"];
        var cardDescription = GetNode<Label>("FrontFace/Description/Description");
        cardDescription.Text = cards[req]["Description"];
        var cardStats = GetNode<Label>("FrontFace/Description/Stats");
        cardStats.Text = cards[req]["ATK"] + " ATK / " + cards[req]["HP"] + " HP";
        var newImage = ImageTexture.CreateFromImage(getImg(cards[req]["Image"]));
        var mesh = GetNode<MeshInstance3D>("FrontFace/Picture");
        var material = mesh.GetActiveMaterial(0) as StandardMaterial3D;
        material!.AlbedoTexture = newImage;
        if (cards[req]["Type"])
        {
            string imageBg;
            switch (cards[req]["Type"])
            {
                case "Spell":
                    imageBg = "";
                    break;
                case "Fighter":
                    imageBg = "";
                    break;
                default:
                    imageBg = "";
                    break;
            }
            var newImageBg = ImageTexture.CreateFromImage(getImg(imageBg));
            var meshBg = GetNode<MeshInstance3D>("FrontFace");
            var materialBg = meshBg.GetActiveMaterial(0) as StandardMaterial3D;
            materialBg!.AlbedoTexture = newImageBg;
        }
    }

    private Godot.Image getImg(string url)
    {
        HttpRequest request = new HttpRequest();
        AddChild(request);
        Godot.Image img = new Godot.Image();
        request.RequestCompleted += (_result, responsecode, header, body) =>
        {
            Error error = img.LoadJpgFromBuffer(body);
        };
        Error error = request.Request(url);
        return img;
    }
}