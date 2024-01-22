using Godot;
using System.Collections.Generic;

public partial class Card : Node3D
{
    //when using this you will do var newcard = new Card(dictionary,cardnum)

    public Area3D collision;
	public bool mouse = false;
	public int placerId;
	public int slot;

	public GameScene.CardObject card;
	public bool set = false;

	public bool down = false;
	public Label3D description;

	public List<string> status = new();
	public Dictionary<string, int> effects = new();

    public List<string> tempStatus = new();
    public Dictionary<string, int> tempEffects = new();

    // Sets the card object elements to display to the player
    public void setCard(GameScene.CardObject _card, Vector3 pos, int playerNum, int _slot) //add added child card
	{
        Hide();

		slot = _slot;
        card = _card;

		description = GetNode<Label3D>("FrontFace/Description/Description");

        Label3D name = GetNode<Label3D>("FrontFace/Name/Name");

		name.Text = card.Name.ToString();

        while (name.Font.GetStringSize(name.Text, name.HorizontalAlignment, -1, name.FontSize).X > name.Width)
        {
			name.FontSize -= 1;
        }

        GetNode<Label3D>("FrontFace/Name/Name").Text = card.Name.ToString();
		GetNode<Label3D>("FrontFace/Name/Rank").Text = card.Rank.ToString();
		
		// Shortens the string if necessary to prevent overflow
		description.Text = card.Description.Length > 133 ? card.SacrificialValue + " - " + card.Description.ToString().Substr(0, 130) + "..." : card.SacrificialValue + " - " + card.Description.ToString();

        string statsText = "";

		if (card.Atk > 0) statsText += card.Atk.ToString() + " ATK";
		if (card.Mana > 0 || card.Hp > 0) statsText += " / ";
		if (card.Hp > 0) statsText += card.Hp.ToString() + " HP";
		if (card.Mana > 0) statsText += " / ";
		if (card.Mana > 0) statsText += card.Mana.ToString() + " Mana";

		GetNode<Label3D>("FrontFace/Description/Stats").Text = statsText;

		if (!IsInsideTree())
			TreeEntered += () =>
			{
				GetImg(card.Img.ToString());
			};
		else
			Show();

		Position = pos;
		Position = new Vector3(Position.X, 0.005f, Position.Z);

		RotationDegrees = new Vector3(0, 180*(playerNum-1), 0);
	}
   
	private void GetImg(string url)
	{
		// Make a request to the online storage of images to load them in

		url = "https://dapchat.github.io/DCGimages/" + url;
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

	public void AddStatus(string name)
	{
		if (GameScene.currentTurn != ServerManager.client.id)
			tempStatus.Add(name);
		else if (!status.Contains(name)) status.Add(name);
    }
	public void AddEffect(string name, int param)
	{
		if (GameScene.currentTurn != ServerManager.client.id)
			tempEffects[name] = param;
		else if (!effects.ContainsKey(name)) effects.Add(name, param);
	}

	public override void _Ready()
	{
		collision = (Area3D)GetNode("Area3D");

		// Check if the mouse is inside the object
		collision.MouseEntered += () => { mouse = true; };
		collision.MouseExited += () => { mouse = false; };
		CardUse();
    }
    public void CardUse()
    {
        var frontFace = GetNode<MeshInstance3D>("FrontFace");
        var originalMaterial = (StandardMaterial3D)frontFace.MaterialOverride;
        var material = new StandardMaterial3D();
        material.Set("albedo_color", new Color(0.8f, 0.8f, 0.8f));
        frontFace.MaterialOverride = material;

        var timer = new Timer();
        timer.WaitTime = 0.1f;
        timer.OneShot = true;
        timer.Timeout += () =>
        {
            frontFace.MaterialOverride = originalMaterial;
            timer.QueueFree();
        };
        AddChild(timer);
        timer.Start();
    }
}