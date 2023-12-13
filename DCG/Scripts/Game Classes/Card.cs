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

	// Sets the card object elements to display to the player
	public void setCard(GameScene.CardObject _card, Vector3 pos, int playerNum) //add added child card
	{
        Hide();

        card = _card;

		description = GetNode<Label3D>("FrontFace/Description/Description");

        GetNode<Label3D>("FrontFace/Name/Name").Text = card.Name.ToString();
		GetNode<Label3D>("FrontFace/Name/Rank").Text = card.Rank.ToString();
		
		// Shortens the string if necessary to prevent overflow
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
            GetImg(card.Img.ToString());
        };

		Position = pos;
		Position = new Vector3(Position.X, 0.005f, Position.Z);

		RotationDegrees = new Vector3(0, 180*(playerNum-1), 0);

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
   
	private void GetImg(string url)
	{
		// Make a request to the online storage of images to load them in

		url = "https://dcgimages.dapchat.repl.co/" + url;
		HttpRequest request = new HttpRequest();
		AddChild(request);
        Image img = new Image();
		request.RequestCompleted += (_result, responsecode, header, body) =>
		{
			Error error = img.LoadJpgFromBuffer(body);

            if (error != Error.Ok)
            {
                Show();
                return;
            }

            // Creates a material from the image and applies it to the card

            var mesh = GetNode<MeshInstance3D>("FrontFace/Picture");
			var material = mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;

			var imageTexture = ImageTexture.CreateFromImage(img);

			if (!GameScene.images.ContainsKey(card.Id.ToString()))
				GameScene.images.Add(card.Id.ToString(), imageTexture);

			material!.AlbedoTexture = imageTexture;

			mesh.MaterialOverride = material;

			Show();
		};

		if (!GameScene.images.ContainsKey(card.Id.ToString()))
		{
			Error error = request.Request(url);
		}
		else
		{
            Show();
            if (GameScene.images[card.Id] == null)
			{
				return;
			}
            var mesh = GetNode<MeshInstance3D>("FrontFace/Picture");
            var material = mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;

            material!.AlbedoTexture = GameScene.images[card.Id.ToString()];

            mesh.MaterialOverride = material;
        }
	}
	public override void _Ready()
	{
		collision = (Area3D)GetNode("Area3D");

		// Check if the mouse is inside the object
		collision.MouseEntered += () => { mouse = true; };
		collision.MouseExited += () => { mouse = false; };
	}
}




