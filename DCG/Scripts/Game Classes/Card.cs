using Godot;

public partial class Card : Node3D
{
	//when using this you will do var newcard = new Card(dictionary,cardnum)

	public Area3D collision;
	public bool mouse = false;


	public GameScene.CardObject card;
	public bool set = false;

	public void setCard(GameScene.CardObject _card, Vector3 pos) //add added child card
	{
		card = _card;

		GetNode<Label3D>("FrontFace/Name/Name").Text = card.Name.ToString();
		GetNode<Label3D>("FrontFace/Name/Rank").Text = card.Rank.ToString();
		GetNode<Label3D>("FrontFace/Description/Description").Text = card.Description.ToString().Substr(0,130) + "...";
		GetNode<Label3D>("FrontFace/Description/Stats").Text = card.Atk.ToString() + " ATK / " + card.Hp.ToString() + " HP";

		getImg(card.Img.ToString());
		Position = pos;
		Position = new Vector3(Position.X, 0.005f, Position.Z);

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
   
	private Image getImg(string url)
	{
		url = "https://publicfiles.dapchat.repl.co/" + url;
		HttpRequest request = new HttpRequest();
		AddChild(request);
        Image img = new Image();
		request.RequestCompleted += (_result, responsecode, header, body) =>
		{
			Error error = img.LoadJpgFromBuffer(body);

			var mesh = GetNode<MeshInstance3D>("FrontFace/Picture");
			var material = mesh.GetActiveMaterial(0) as StandardMaterial3D;
			material!.AlbedoTexture = ImageTexture.CreateFromImage(img);
		};
		Error error = request.Request(url);

		Show();

		return img;
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
		{
			if (!mouse || set)
			{
				GameScene.ReturnView();
                GetNode<Label3D>("FrontFace/Description/Description").Show();
                set = false;
				return;
			}
			GameScene.ViewCard(Position, card, GetNode<Label3D>("FrontFace/Description/Description"));
            set = true;
		}
	}
	public override void _Ready()
	{
		Hide();

		collision = (Area3D)GetNode("Area3D");

		collision.MouseEntered += () => { mouse = true; };
		collision.MouseExited += () => { mouse = false; };
	}
}




