using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        int player = 1;
        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate().Duplicate();

		string slot = _action.card.Type.Contains("Spell") ? "Spell" : "Slot";

		if (_action.placerId != ServerManager.client.id)
		{
			player = 2;
		}

        var cardGlobalPosition = sceneTree.GetNode<MeshInstance3D>("Player" + player + "/" + slot + _action.slot.ToString()).GlobalPosition;

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

        tween.Parallel().TweenProperty(curCamera, "rotation_degrees", new Vector3(-90,yRot,0), card.RotationDegrees.Y == 180 ? 1 : .5);

        tween.Parallel().TweenProperty(curCamera, "global_position", new Vector3(cardPos.X, cardPos.Y + 2, cardPos.Z), card.RotationDegrees.Y == 180 ? 1 : .5);

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
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam1Rot, card.RotationDegrees.Y == 180 ? 1 : .5);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam1Pos, card.RotationDegrees.Y == 180 ? 1 : .5);
        }
        else
        {
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam2Rot, card.RotationDegrees.Y == 180 ? 1 : .5);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam2Pos, card.RotationDegrees.Y == 180 ? 1 : .5);
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
				if (card.mouse && !card.set && (card != zoomed))
				{
					zoomed = card;
					ViewCard(card.Position, card, card.description);
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
                ReturnView(c);
				zoomed = null;
			}else if (c == null && !skip)
			{
				if (cardObject == null) return;

				var eventMouseButton = (InputEventMouseButton) @event;

                var from = curCamera.ProjectRayOrigin(eventMouseButton.Position);
                var to = from + curCamera.ProjectRayNormal(eventMouseButton.Position) * 1000;

                var spaceState = GetWorld3D().DirectSpaceState;
                var query = PhysicsRayQueryParameters3D.Create(from, to);
                var result = spaceState.IntersectRay(query);

				if (result.Count == 0) return;

				Node3D collider = (Node3D)((Node3D)result["collider"]).GetParent();

				if (collider.GetParent().Name == "Player2") return;

				if (cardObject.Type == "Spell")
				{
					if (!collider.Name.ToString().Contains(cardObject.Type.ToString())) { return; }
				}
				else if (collider.Name.ToString().Contains("Spell")) { return; }

				int slot = 0;

				try
				{
					slot = int.Parse(Regex.Match(collider.Name, @"\d+").Value);
				}catch (Exception)
				{
					return;
				}

                PlaceCard(new CAP { placerId = 0, card = cardObject, action = "place", slot = slot });
            }
        }
    }

    public override void _Ready()
	{
		Button buttonCamera = (Button)GetNode("/root/Game/CanvasLayer/Control/ChangeView");
        Button buttonHand = (Button)GetNode("/root/Game/CanvasLayer/Control/Hand");
		Button buttonCloseHand = (Button)GetNode("/root/Game/CanvasLayer/Control/PlayerHandActivation/Cancel");
        description = (RichTextLabel)GetNode("/root/Game/CanvasLayer/Control/Desc");
        var hand = (ColorRect)GetNode("/root/Game/CanvasLayer/Control/PlayerHandActivation");
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
            changeScene = false;
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
        }

		base._Process(delta);
	}
}
