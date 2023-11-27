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
    static bool HandShown = false;
    static Camera3D curCamera = null;

	static Vector3 cam1Pos = new(0, 9, 10);
	static Vector3 cam2Pos = new(0, 10, 0);
	static Vector3 cam1Rot = new(-45, 0, 0);
	static Vector3 cam2Rot = new(-90, 0, 0);
	static RichTextLabel description;

	static GameScene sceneTree;

	static Vector3 camRot;

	static Tween tween;
	static List<Card> cards = new List<Card>();

    public static CardObject cardObject = null;
	public static bool changeScene = false;
	public static Card zoomed = null;

	public static void PlaceCard(CAP _action)
	{
        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate().Duplicate();

		string slot = _action.card.Type.Contains("Spell") ? "Spell" : "Slot";

        var cardGlobalPosition = sceneTree.GetNode<MeshInstance3D>("Player1/" + slot + _action.slot.ToString()).GlobalPosition;

		sceneTree.CallDeferred(Node.MethodName.AddChild, thescene);

		Card c = thescene as Card;

		cards.Add(c);
		
        c.setCard(_action.card, cardGlobalPosition);
    }

	public static void ViewCard(Vector3 cardPos, Card card, Label3D node)
	{
		//curCamera.RotationDegrees = new Vector3(-90, 0, 0);
		//curCamera.GlobalPosition = new Vector3(cardPos.X, cardPos.Y + 2, cardPos.Z);

		if (tween != null && tween.IsRunning())
			tween.Kill();

        tween = curCamera.CreateTween();

		description.Hide();

		tween.Finished += () =>
		{
            camRot.Y = card.RotationDegrees.Y;
            description.Show();
			node.Hide();
        };

		float yRot;

		if (camRot.Y != card.RotationDegrees.Y)
		{
			yRot = card.RotationDegrees.Y;
		}
		else
		{
			yRot = curCamera.RotationDegrees.Y;
        }

        tween.Parallel().TweenProperty(curCamera, "rotation_degrees", new Vector3(-90,yRot,0), card.RotationDegrees.Y == 180 ? 2 : 1);

        tween.Parallel().TweenProperty(curCamera, "global_position", new Vector3(cardPos.X, cardPos.Y + 2, cardPos.Z), card.RotationDegrees.Y == 180 ? 2 : 1);

        description.ScrollToLine(0);
		description.Text = card.card.Description;
	}

	public static void ReturnView(Card card)
	{
		if (tween != null && tween.IsRunning())
			tween.Kill();

        tween = curCamera.CreateTween();

		//tween.Finished += () => ;

		description.Hide();

		if (!CameraView)
        {
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam1Rot, card.RotationDegrees.Y == 180 ? 2 : 1);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam1Pos, card.RotationDegrees.Y == 180 ? 2 : 1);
        }
        else
        {
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam2Rot, card.RotationDegrees.Y == 180 ? 2 : 1);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam2Pos, card.RotationDegrees.Y == 180 ? 2 : 1);
        }

		camRot.Y = curCamera.Position.Y;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Left_Click") && !@event.IsEcho())
        {
			Card c = null;
			bool skip = false;

			foreach (Card card in cards) {
				if (card.mouse && !card.set && (card != GameScene.zoomed))
				{
					GameScene.zoomed = card;
					GameScene.ViewCard(card.Position, card, card.description);
					card.set = true;
					skip = true;
					continue;
				}

				if (card.set)
				{
					if (zoomed == card)
					{
						c = card;
                    }
					card.description.Show();
					card.set = false;
					continue;
				}
			}

			if (c != null && !skip)
			{
				GameScene.ReturnView(c);
				GameScene.zoomed = null;
			}
        }
    }

    public override void _Ready()
	{
		Button buttonCamera = (Button)GetNode("/root/Game/CanvasLayer/Control/ChangeView");
        Button buttonHand = (Button)GetNode("/root/Game/CanvasLayer/Control/Hand");
		Button buttonCloseHand = (Button)GetNode("/root/Game/CanvasLayer/Control/PlayerHand/Cancel");
        description = (RichTextLabel)GetNode("/root/Game/CanvasLayer/Control/Desc");
        var hand = (ColorRect)GetNode("/root/Game/CanvasLayer/Control/PlayerHand");
        sceneTree = this;

        curCamera = new Camera3D();

		AddChild(curCamera);

        curCamera.GlobalPosition = cam1Pos;
		curCamera.GlobalRotationDegrees = cam1Rot;

        curCamera.MakeCurrent();

		buttonCamera.ButtonDown += () =>
		{
			if (tween != null && tween.IsRunning())
				tween.Kill();

			tween = curCamera.CreateTween();

			if (CameraView)
			{
				tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam1Rot, 1);
				tween.Parallel().TweenProperty(curCamera, "global_position", cam1Pos, 1);
				CameraView = false;
			}
			else
			{
				tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam2Rot, 1);
				tween.Parallel().TweenProperty(curCamera, "global_position", cam2Pos, 1);
				CameraView = true;
			}
		};	
		buttonHand.ButtonDown += () =>
		{
			hand.Show();
			HandShown = true;
			buttonCloseHand.Disabled = false;
			buttonHand.Disabled = true;
		};
		buttonCloseHand.ButtonDown += () =>
		{
			hand.Hide();
			HandShown = false;
			buttonCloseHand.Disabled = true;
            buttonHand.Disabled = false;
        };

    }

	public override void _Process(double delta)
	{
		if (changeScene)
		{
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
			changeScene = false;
        }

		base._Process(delta);
	}
}
