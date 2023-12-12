using Godot;
using MongoDB.Bson;
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

	public static GameScene sceneTree;

	static Vector3 camRot;

	static Tween tween;
	static List<Card> cards = new List<Card>();
	public static List<CardObject> hand = new();
	public static List<CAP> actionQueue = new();

    public static CardObject cardObject = null;
	public static bool changeScene = false;
	public static Card zoomed = null;

	public static D2Card selectedHand = null;
	static D2Card lastSelectedHand = null;

	public static Dictionary<string, ImageTexture> images = new();

	public static int gridSeparation = 250;

	public static void PlaceCard(CAP _action)
	{
        int player = 1;
        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate().Duplicate();

		// Gets which slot to place it in
		string slot = _action.card.Type.Contains("Spell") ? "Spell" : "Slot";

		if (_action.placerId != ServerManager.client.id)
		{
			player = 2;
		}

        var cardGlobalPosition = sceneTree.GetNode<MeshInstance3D>("Player" + player + "/" + slot + _action.slot.ToString()).GlobalPosition;

		sceneTree.CallDeferred(Node.MethodName.AddChild, thescene);

		Card c = thescene as Card;

		cards.Add(c);
		
        c.setCard(_action.card, cardGlobalPosition, player);
    }

	public static void RemoveFromHand(CardObject card)
	{
		var grid = (GridContainer)sceneTree.GetNode("CanvasLayer/Control/PlayerHand/GridContainer");

		foreach (D2Card c in grid.GetChildren())
		{
			if (c.card.Id == card.Id)
			{
				if (lastSelectedHand != null && lastSelectedHand.card.Id == c.card.Id)
				{
					lastSelectedHand = null;
				}

				foreach (var _c in ServerManager.client.hand)
				{
					if (_c.Id == card.Id)
					{
						ServerManager.client.hand.Remove(_c);
						break;
					}
				}

				grid.RemoveChild(c);
				c.QueueFree();

				gridSeparation = 250;

                while ((ServerManager.client.hand.Count * gridSeparation + (225 - gridSeparation)) > sceneTree.GetViewport().GetVisibleRect().Size.X)
                {
                    gridSeparation -= 1;
                }

                grid.AddThemeConstantOverride(new StringName("h_separation"), gridSeparation);

                break;
			}
		}
	}

	public static void AddToHand(CardObject card)
	{
        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/2d_card.tscn").Instantiate().Duplicate();

		var grid = (GridContainer)sceneTree.GetNode("CanvasLayer/Control/PlayerHand/GridContainer");

        grid.CallDeferred(Node.MethodName.AddChild, thescene);

		while ((ServerManager.client.hand.Count * gridSeparation + (225 - gridSeparation)) > sceneTree.GetViewport().GetVisibleRect().Size.X)
		{
			gridSeparation -= 1;
		}

        grid.AddThemeConstantOverride(new StringName("h_separation"), gridSeparation);

        D2Card c = thescene as D2Card;

		c.setCard(card);
    }

	// Zoom into the card
	public static void ViewCard(Vector3 cardPos, Card card, Label3D node)
	{
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

		// Set the scrollable description to the full description of the card (not shortened)
        description.ScrollToLine(0);
		description.Text = card.card.Description;
	}

	// Return to normal view
	public static void ReturnView(Card card)
	{
		if (tween != null && tween.IsRunning())
			tween.Kill();

        tween = curCamera.CreateTween();

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
		// Check if the player clicked
        if (@event.IsActionPressed("Left_Click") && !@event.IsEcho())
        {
			if (HandShown)
			{
				Button buttonHand = (Button)GetNode("/root/Game/CanvasLayer/Control/Hand");
				var hand = (Container)GetNode("/root/Game/CanvasLayer/Control/PlayerHand");

				if (selectedHand != null)
				{
					selectedHand.keepShown = true;

					if (lastSelectedHand == selectedHand)
					{
						cardObject = selectedHand.card;

						selectedHand = null;
						lastSelectedHand.keepShown = false;
						lastSelectedHand.ReturnCard();

						hand.Hide();
						HandShown = false;
						buttonHand.Disabled = false;

						return;
					}

                    cardObject = null;

                    if (lastSelectedHand != null && lastSelectedHand != selectedHand)
					{
						lastSelectedHand.keepShown = false;
						lastSelectedHand.ReturnCard();
					}

					lastSelectedHand = selectedHand;

					return;
				}
				else
				{
					cardObject = null;

					if (lastSelectedHand != null)
					{
						lastSelectedHand.keepShown = false;
						lastSelectedHand.ReturnCard();

						lastSelectedHand = selectedHand;

						return;
					}

					hand.Hide();
					HandShown = false;
					buttonHand.Disabled = false;

					return;
				}
			}

            Card c = null;
			bool skip = false;

			// Check each card to see if there was one that was clicked
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

			// If none were clicked then return view
			if (c != null && !skip)
			{
                ReturnView(c);
				zoomed = null;
			}else if (c == null && !skip)
			{
				// Check which slot was clicked
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

                ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = cardObject, action = "place", slot = slot }));
            }
        }
    }

    public override void _Ready()
	{
		Button buttonCamera = (Button)GetNode("/root/Game/CanvasLayer/Control/ChangeView");
        Button buttonHand = (Button)GetNode("/root/Game/CanvasLayer/Control/Hand");
        description = (RichTextLabel)GetNode("/root/Game/CanvasLayer/Control/Desc");
        var hand = (Container)GetNode("/root/Game/CanvasLayer/Control/PlayerHand");
        sceneTree = this;

        curCamera = new Camera3D();

		AddChild(curCamera);

        curCamera.GlobalPosition = cam1Pos;
		curCamera.GlobalRotationDegrees = cam1Rot;

        curCamera.MakeCurrent();

		// Move between the two camera angles
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
			buttonHand.Disabled = true;
		};
    }

	public override void _Process(double delta)
	{
		if (changeScene)
		{
            changeScene = false;
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
        }

		while (hand.Count > 0)
		{
			var curCard = hand[0];
			AddToHand(curCard);
			hand.RemoveAt(0);
		}

		while (actionQueue.Count > 0)
		{
			var cap = actionQueue[0];

			switch (cap.action)
			{
				case "place":
					PlaceCard(cap);
					break;
				case "hadd":
					AddToHand(cap.card);
					break;
				case "hremove":
					RemoveFromHand(cap.card);
					break;
			}

			actionQueue.RemoveAt(0);
		}

		base._Process(delta);
	}
}
