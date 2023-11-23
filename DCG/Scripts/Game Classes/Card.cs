using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Card : Node3D
{
	//when using this you will do var newcard = new Card(dictionary,cardnum)

	public Area3D collision;
	public bool mouse = false;

	public GameScene.CardObject card;
	public bool set = false;

	public bool down = false;
	public Label3D description;

	public void setCard(GameScene.CardObject _card, Vector3 pos) //add added child card
	{
		card = _card;

		description = GetNode<Label3D>("FrontFace/Description/Description");

        GetNode<Label3D>("FrontFace/Name/Name").Text = card.Name.ToString();
		GetNode<Label3D>("FrontFace/Name/Rank").Text = card.Rank.ToString();
		description.Text = card.Description.Length > 133 ? card.Description.ToString().Substr(0, 130) + "..." : card.Description.ToString();

        string statsText = "";

		if (card.Atk > 0) statsText += card.Atk.ToString() + " ATK";
		if (card.Mana > 0 || card.Hp > 0) statsText += " / ";
		if (card.Hp > 0) statsText += card.Hp.ToString() + " HP";
		if (card.Mana > 0) statsText += " / ";
		if (card.Mana > 0) statsText += card.Mana.ToString() + " Mana";

		GetNode<Label3D>("FrontFace/Description/Stats").Text = statsText;

		TreeEntered += () =>
		{
            getImg(card.Img.ToString());
        };

		Position = pos;
		Position = new Vector3(Position.X, 0.005f, Position.Z);

		RotationDegrees = new Vector3(0, 0, 0);

		//if (card.Type != null)
		//{
		//    string imageBg;
		//    switch (card.Type)
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
   
	private void getImg(string url)
	{
		url = "https://publicfiles.dapchat.repl.co/" + url;
		HttpRequest request = new HttpRequest();
		AddChild(request);
        Image img = new Image();
		request.RequestCompleted += (_result, responsecode, header, body) =>
		{
			Error error = img.LoadJpgFromBuffer(body);

			var mesh = GetNode<MeshInstance3D>("FrontFace/Picture");
			var material = mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
			material!.AlbedoTexture = ImageTexture.CreateFromImage(img);

			mesh.MaterialOverride = material;

            Show();
        };
		Error error = request.Request(url);
	}
	public override void _Ready()
	{
		Hide();

		collision = (Area3D)GetNode("Area3D");

		collision.MouseEntered += () => { mouse = true; };
		collision.MouseExited += () => { mouse = false; };
	}
}




