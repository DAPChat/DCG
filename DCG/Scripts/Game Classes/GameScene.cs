using Godot;
using System.Collections.Generic;

public partial class GameScene : Node3D 
{
	public class CardObject
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Rank { get; set; }
		public double SacrificialValue { get; set; }
		public int Atk { get; set; }
		public int Hp { get; set; }
		public int Mana { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
		public string Img { get; set; }
		public string Pack { get; set; }
		public string CurrentStatus { get; set; }
	}

	static bool CameraView = false;
	static Camera3D curCamera = null;

	static Vector3 cam1Pos = new(0, 9, 10);
	static Vector3 cam2Pos = new(0, 10, 0);
	static Vector3 cam1Rot = new(-45, 0, 0);
	static Vector3 cam2Rot = new(-90, 0, 0);
	static RichTextLabel description;

	static Vector3 camPos = new();
	static Vector3 camRot;

	static Tween rotTween;
	static Tween posTween;

    public static CardObject cardObject = null;
	public static bool changeScene = false;

	public void PlaceCard(CAP action)
	{
		List<CardObject> list = new List<CardObject>() { action.card };

        //var thescene = ResourceLoader.Load<CSharpScript>("res://Scripts/Card.cs").New();

        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate();

		string slot = action.card.Type.Contains("Spell") ? "Spell" : "Slot";

        var cardGlobalPosition = GetNode<MeshInstance3D>("Player1/" + slot + action.slot.ToString()).GlobalPosition;

        AddChild(thescene);

        Card c = thescene as Card;

        c.setCard(action.card, cardGlobalPosition);
    }

	public static void ViewCard(Vector3 cardPos, CardObject card, Label3D node)
	{
        //curCamera.RotationDegrees = new Vector3(-90, 0, 0);
        //curCamera.GlobalPosition = new Vector3(cardPos.X, cardPos.Y + 2, cardPos.Z);

        rotTween = curCamera.CreateTween();
        posTween = curCamera.CreateTween();

		rotTween.Finished += () =>
		{
			description.Show();
			node.Hide();
		};

        rotTween.TweenProperty(curCamera, "rotation_degrees", new Vector3(-90,0,0), 1);
		posTween.TweenProperty(curCamera, "global_position", new Vector3(cardPos.X, cardPos.Y + 2, cardPos.Z), 1);

		description.ScrollToLine(0);
		description.Text = card.Description;
	}

	public static void ReturnView()
	{
        rotTween = curCamera.CreateTween();
        posTween = curCamera.CreateTween();

		description.Hide();

		if (!CameraView)
        {
            rotTween.TweenProperty(curCamera, "rotation_degrees", cam1Rot, 1);
            posTween.TweenProperty(curCamera, "global_position", cam1Pos, 1);
        }
        else
        {
            rotTween.TweenProperty(curCamera, "rotation_degrees", cam2Rot, 1);
            posTween.TweenProperty(curCamera, "global_position", cam2Pos, 1);
        }
    }

	public override void _Ready()
	{
		Button button = (Button)GetNode("/root/Game/CanvasLayer/Control/ChangeView");
		description = (RichTextLabel)GetNode("/root/Game/CanvasLayer/Control/Desc");

        curCamera = new Camera3D();

		AddChild(curCamera);

        curCamera.GlobalPosition = cam1Pos;
		curCamera.RotationDegrees = cam1Rot;

        curCamera.MakeCurrent();

		button.ButtonDown += () =>
		{
            rotTween = curCamera.CreateTween();
            posTween = curCamera.CreateTween();

            if (CameraView)
            {
                rotTween.TweenProperty(curCamera, "rotation_degrees", cam1Rot, 1);
                posTween.TweenProperty(curCamera, "global_position", cam1Pos, 1);
				CameraView = false;
            }
            else
            {
                rotTween.TweenProperty(curCamera, "rotation_degrees", cam2Rot, 1);
                posTween.TweenProperty(curCamera, "global_position", cam2Pos, 1);
				CameraView = true;
            }
        };
	}

	public override void _Process(double delta)
	{
		if(cardObject != null)
		{
			PlaceCard(new CAP { card = cardObject, slot = 1 });
			cardObject = null;
		}

		if (changeScene)
		{
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
			changeScene = false;
        }

		base._Process(delta);
	}
}
