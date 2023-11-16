using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

public partial class Card : Node3D
{
	private const float RayLength = 1000.0f;
	//when using this you will do var newcard = new Card(dictionary,cardnum)

	public Area3D collision;
	public bool mouse = false;

	public void setCard(Main.CardObject[] cards, int req,dynamic pos) //add added child card
	{
		var cardName = GetNode<Label3D>("FrontFace/Name/Name");
		cardName.Text = cards[req].Name.ToString();
		var cardRank = GetNode<Label3D>("FrontFace/Name/Rank");
		cardRank.Text = cards[req].Rank.ToString();
		var cardDescription = GetNode<Label3D>("FrontFace/Description/Description");
		cardDescription.Text = cards[req].Description.ToString();
		var cardStats = GetNode<Label3D>("FrontFace/Description/Stats");

		cardStats.Text = cards[req].Atk.ToString() + " ATK / " + cards[req].Hp.ToString() + " HP";
		getImg(cards[req].Img.ToString());
		Position = pos;
		Position = new Vector3(Position.X, 0.005f, Position.Z);

		//if (cards[req].Type != null)
		//{
		//    string imageBg;
		//    switch (cards[req].Type)
		//    {
		//        case "Spell":
		//            imageBg = "";
		//            break;
		//        case "Fighter":
		//            imageBg = "";
		//            break;
		//        default:
		//            imageBg = "";
		//            break;
		//    }
		//    var newImageBg = ImageTexture.CreateFromImage(getImg(imageBg));
		//    var meshBg = GetNode<MeshInstance3D>("FrontFace");
		//    var materialBg = meshBg.GetActiveMaterial(0) as StandardMaterial3D;
		//    materialBg!.AlbedoTexture = newImageBg;
		//}
		

	}
   
	private Godot.Image getImg(string url)
	{
		url = "https://publicfiles.dapchat.repl.co/" + url;
		HttpRequest request = new HttpRequest();
		AddChild(request);
		Godot.Image img = new Godot.Image();
		request.RequestCompleted += (_result, responsecode, header, body) =>
		{
			Error error = img.LoadJpgFromBuffer(body);

			var mesh = GetNode<MeshInstance3D>("FrontFace/Picture");
			var material = mesh.GetActiveMaterial(0) as StandardMaterial3D;
			material!.AlbedoTexture = ImageTexture.CreateFromImage(img);
		};
		Error error = request.Request(url);
		return img;
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
		{
			if (!mouse) return;
			

		}
	}
	public override void _Ready()
	{
		collision = (Area3D)GetNode("Area3D");

		collision.MouseEntered += () => { mouse = true; };
		collision.MouseExited += () => { mouse = false; };
	}
}




