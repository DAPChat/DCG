using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using packets;

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
    static Vector3 camRot;

    static double zoomFlip = 1;
    static double zoomNorm = .5;
    static double zoomView = 1;

    public static double zMultiplier = 0;

    static Tween tween;
    static RichTextLabel description;
    static D2Card lastSelectedHand = null;

    static List<Card> cards = new();

    public static GameScene sceneTree;

    public static List<CardObject> hand = new();
    public static List<CAP> actionQueue = new();

    public static CardObject cardObject = null;
    public static Card zoomed = null;

    public static bool changeScene = false;
    public static bool selectMode = false;

    public static Action choose = null;

    public static D2Card selectedHand = null;

    public static Dictionary<string, ImageTexture> images = new();

    public static int gridSeparation = 300;

    public static int currentTurn = 0;
    public static int currentPhase = 0;

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

        c.placerId = _action.placerId;
        c.setCard(_action.card, cardGlobalPosition, player, _action.slot);
    }

    public static void UpdateCard(CAP _action)
    {
        foreach (var card in sceneTree.GetChildren())
        {
            if (card is not Card) continue;

            Card c = (Card)card;

            if (c.placerId == _action.targetId && c.card.Id == _action.card.Id && _action.slot == (c.slot - 1))
            {
                c.setCard(_action.card, c.Position, c.placerId == ServerManager.client.id ? 1 : 2, c.slot);
                return;
            }
        }
    }

    public static void RemoveCard(CAP _action)
    {
        foreach (var card in sceneTree.GetChildren())
        {
            if (card is not Card) continue;

            Card c = (Card)card;

            if (c.placerId == _action.targetId && c.card.Id == _action.card.Id && _action.slot == (c.slot-1))
            {
                cards.Remove(c);
                c.QueueFree();
                return;
            }
        }
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
        ServerManager.client.hand.Add(card);

        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/2d_card.tscn").Instantiate().Duplicate();

        var grid = (GridContainer)sceneTree.GetNode("CanvasLayer/Control/PlayerHand/GridContainer");

        grid.CallDeferred(Node.MethodName.AddChild, thescene);

        gridSeparation = 300;

        while ((ServerManager.client.hand.Count * gridSeparation + ((225) - gridSeparation)) > sceneTree.GetViewport().GetVisibleRect().Size.X)
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
        VBoxContainer vbox = (VBoxContainer)sceneTree.GetNode("CanvasLayer/Control/Actions");
        Button actionButton = (Button)ResourceLoader.Load<PackedScene>("res://Scenes/action.tscn").Instantiate();

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

        tween.Parallel().TweenProperty(curCamera, "rotation_degrees", new Vector3(-90, yRot, 0), card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);

        tween.Parallel().TweenProperty(curCamera, "global_position", new Vector3(cardPos.X, cardPos.Y + 2, cardPos.Z), card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);

        // Set the scrollable description to the full description of the card (not shortened)
        description.ScrollToLine(0);
        description.Text = card.card.Description;

        foreach (var child in vbox.GetChildren())
        {
            child.QueueFree();
        }

        if (zoomed.placerId != ServerManager.client.id || (currentPhase != 2 && currentTurn != ServerManager.client.id)) return;

        Type type = null;

        try
        {
            type = Type.GetType("card." + card.card.Name.Replace(' ', '_'));
        }
        catch
        {
            return;
        }

        foreach (var t in type.GetNestedTypes())
        {
            Action cardClass = (Action)Activator.CreateInstance(t);

            Button action = (Button)actionButton.Duplicate();

            action.Text = cardClass.name;

            action.Pressed += () =>
            {
                if (selectMode && currentPhase == 2)
                {
                    selectMode = false;
                    cardClass.Run(zoomed.card);
                }
            };

            vbox.CallDeferred(Node.MethodName.AddChild, action);
        }
    }

    // Return to normal view
    public static void ReturnView(Card card)
    {
        if (card == null) return;

        VBoxContainer vbox = (VBoxContainer)sceneTree.GetNode("CanvasLayer/Control/Actions");

        foreach (var child in vbox.GetChildren())
        {
            child.QueueFree();
        }

        card.description.Show();
        card.set = false;

        zoomed = null;

        if (tween != null && tween.IsRunning())
            tween.Kill();

        tween = curCamera.CreateTween();

        description.Hide();

        if (!CameraView)
        {
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam1Rot, card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam1Pos, card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
        }
        else
        {
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam2Rot, card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam2Pos, card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
        }

        camRot.Y = curCamera.Position.Y;
    }

    public static void QuietReturn()
    {
        VBoxContainer vbox = (VBoxContainer)sceneTree.GetNode("CanvasLayer/Control/Actions");

        foreach (var child in vbox.GetChildren())
        {
            child.QueueFree();
        }

        if (tween != null && tween.IsRunning())
            tween.Kill();

        tween = curCamera.CreateTween();

        zoomed.description.Show();

        description.Hide();

        if (!CameraView)
        {
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam1Rot, curCamera.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam1Pos, curCamera.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
        }
        else
        {
            tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam2Rot, curCamera.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
            tween.Parallel().TweenProperty(curCamera, "global_position", cam2Pos, curCamera.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);
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

            if (choose != null)
            {
                var eventMouseButton = (InputEventMouseButton)@event;

                var from = curCamera.ProjectRayOrigin(eventMouseButton.Position);
                var to = from + curCamera.ProjectRayNormal(eventMouseButton.Position) * 1000;

                var spaceState = sceneTree.GetWorld3D().DirectSpaceState;
                var query = PhysicsRayQueryParameters3D.Create(from, to);
                var result = spaceState.IntersectRay(query);

                if (result.Count == 0) return;

                Node3D collider = (Node3D)((Node3D)result["collider"]).GetParent();

                if (collider.GetParent().Name != "Player2") return;

                Card occupied = null;

                foreach (Card curCard in cards)
                {
                    if (curCard.mouse)
                    {
                        occupied = curCard;
                    }
                }

                if (occupied != null)
                {
                    if (occupied.card.Type.Contains("Spell")) return;

                    choose.Run(zoomed.card, occupied.slot - 1);
                    choose = null;
                    ReturnView(zoomed);
                }

                return;
            }

            Card c = null;
            bool skip = false;

            // Check each card to see if there was one that was clicked
            foreach (Card card in cards)
            {
                if (!selectMode && card.mouse && !card.set && (card != zoomed))
                {
                    zoomed = card;
                    ViewCard(card.Position, card, card.description);
                    card.set = true;
                    skip = true;
                    continue;
                }
                else if (!selectMode && card.mouse && card.set && card == zoomed && card.placerId == ServerManager.client.id)
                {
                    selectMode = true;
                    skip = true;
                    break;
                }
                else if (selectMode && card.set && card == zoomed && card.mouse)
                {
                    selectMode = false;
                    skip = true;
                    break;
                }
                else if (selectMode)
                {
                    skip = true;
                    break;
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
            }
            else if (c == null && !skip && currentTurn == ServerManager.client.id && currentPhase == 1)
            {
                // Check which slot was clicked
                if (cardObject == null) return;

                var eventMouseButton = (InputEventMouseButton)@event;

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

                int slot;

                try
                {
                    slot = int.Parse(Regex.Match(collider.Name, @"\d+").Value);
                }
                catch (Exception)
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
            if (selectMode)
            {
                selectMode = false;

                if (zoomed != null)
                {
                    ReturnView(zoomed);
                }
            }

            if (tween != null && tween.IsRunning())
                tween.Kill();

            tween = curCamera.CreateTween();

            if (CameraView)
            {
                tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam1Rot, zoomView * zMultiplier);
                tween.Parallel().TweenProperty(curCamera, "global_position", cam1Pos, zoomView * zMultiplier);
                CameraView = false;
            }
            else
            {
                tween.Parallel().TweenProperty(curCamera, "rotation_degrees", cam2Rot, zoomView * zMultiplier);
                tween.Parallel().TweenProperty(curCamera, "global_position", cam2Pos, zoomView * zMultiplier);
                CameraView = true;
            }
        };
        buttonHand.ButtonDown += () =>
        {
            if (selectMode)
            {
                selectMode = false;

                if (zoomed != null)
                {
                    ReturnView(zoomed);
                }
            }

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
                case "update":
                    UpdateCard(cap);
                    break;
                case "remove":
                    RemoveCard(cap);
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
