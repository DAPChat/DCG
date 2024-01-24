using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using packets;
using card;
using System.Linq;

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
        public string Class { get; set; }
        public string Type { get; set; }
        public string Img { get; set; }
        public string Pack { get; set; }
        public List<string> StatusName { get; set; }
        public List<int> StatusLength { get; set; }
        public List<string> EffectName { get; set; }
        public List<int> EffectParam { get; set; }
        public List<int> EffectLength { get; set; }

        public CardObject Clone()
        {
            return new CardObject
            {
                Id = Id,
                Name = Name,
                Rank = Rank,
                SacrificialValue = SacrificialValue,
                Atk = Atk,
                Hp = Hp,
                Mana = Mana,
                Description = Description,
                Class = Class,
                Type = Type,
                Img = Img,
                Pack = Pack,
                StatusName = StatusName,
                StatusLength = StatusLength,
                EffectName = EffectName,
                EffectParam = EffectParam,
                EffectLength = EffectLength
            };
        }
    }

    static bool CameraView = false;
    public static bool HandShown = false;
    static Camera3D curCamera = null;

    static Vector3 cam1Pos = new(0, 9, 10);
    static Vector3 cam2Pos = new(0, 10, 0);
    static Vector3 cam1Rot = new(-45, 0, 0);
    static Vector3 cam2Rot = new(-90, 0, 0);
    static Vector3 camRot;

    static double zoomFlip = 1.5;
    static double zoomNorm = .5;
    static double zoomView = 1;

    public static double zMultiplier = 0;

    static Tween tween;
    static RichTextLabel description;
    static D2Card lastSelectedHand = null;

    public static List<Card> cards = new();

    public static GameScene sceneTree;

    public static List<CardObject> hand = new();
    public static List<CardObject> forgotten = new();
    public static List<CardObject> unforgotten = new();

    public static List<CAP> actionQueue = new();
    public static List<PUP> playerQueue = new();
    public static List<EUP> effectQueue = new();
    public static string winner = null;

    public static CardObject cardObject = null;
    public static Card zoomed = null;

    public static bool changeScene = false;

    public static Action chooseTarget = null;
    public static Action targetPlace = null;

    public static D2Card selectedHand = null;

    public static Dictionary<string, ImageTexture> images = new();

    public static int gridSeparation = 300;

    public static int currentTurn = 0;
    public static int currentPhase = 0;

    public static double matchSacrifice;
    public static List<int> sacrificed = new();
    public static double sacrificialAmt;
    public static Dictionary<CardObject, int> toSummon = new();

    public static BaseCard summonCard = null;
    public static Dictionary<CardObject, int> cardSummon = new();
    public static List<CardObject> chosen = new();

    public static void PlaceCard(CAP _action)
    {
        int player = 1;
        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate().Duplicate();

        // Gets which slot to place it in
        string slot = _action.card.Class.Contains("Spell") ? "Spell" : "Slot";

        if (_action.placerId != ServerManager.client.id)
        {
            player = 2;
        }

        var cardGlobalPosition = sceneTree.GetNode<MeshInstance3D>("Player" + player + "/" + slot + (_action.targetSlot+1).ToString()).GlobalPosition;

        sceneTree.CallDeferred(Node.MethodName.AddChild, thescene);

        Card c = thescene as Card;

        cards.Add(c);

        c.placerId = _action.placerId;
        c.setCard(_action.card, cardGlobalPosition, player, _action.targetSlot);
    }

    public static void UpdateCard(CAP _action)
    {
        foreach (var card in sceneTree.GetChildren())
        {
            if (card is not Card) continue;

            Card c = (Card)card;

            if (c.placerId == _action.targetId && c.card.Id == _action.card.Id && _action.targetSlot == (c.slot))
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

            if (c.placerId == _action.targetId && c.card.Id == _action.card.Id && _action.targetSlot == (c.slot))
            {
                if (c == zoomed) ReturnView(c);

                cards.Remove(c);
                c.QueueFree(); // change this to moving it to stack ontop of forggoten or unforgotten piles
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

                gridSeparation = 300;

                while ((ServerManager.client.hand.Count * gridSeparation + ((225) - gridSeparation)) > sceneTree.GetViewport().GetVisibleRect().Size.X)
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

    public static void ShowForgotten()
    {
        ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Show();
        GridContainer container = (GridContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands/GridContainer");

        ClearTempHand();

        foreach (var dead in forgotten)
        {
            var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/2d_card.tscn").Instantiate().Duplicate();

            container.AddChild(thescene);

            D2Card c = thescene as D2Card;

            c.setCard(dead);
        }

        HandShown = true;
    }

    public static void ClearTempHand()
    {
        ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Show();
        GridContainer container = (GridContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands/GridContainer");

        foreach (var child in container.GetChildren())
        {
            container.RemoveChild(child);
            child.QueueFree();
        }
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

        tween.Parallel().TweenProperty(curCamera, "rotation_degrees", new Vector3(-90, yRot, 0), card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);

        tween.Parallel().TweenProperty(curCamera, "global_position", new Vector3(cardPos.X, cardPos.Y + 2, cardPos.Z), card.RotationDegrees.Y == 180 ? zoomFlip * zMultiplier : zoomNorm * zMultiplier);

        // Set the scrollable description to the full description of the card (not shortened)
        description.ScrollToLine(0);
        description.Text = card.card.SacrificialValue + " - " + card.card.Description;

        ShowActions(card);
    }

    public static void ShowActions(Card card)
    {
        if (zoomed != card || chooseTarget != null) return;

        VBoxContainer vbox = (VBoxContainer)sceneTree.GetNode("CanvasLayer/Control/Actions");
        Button actionButton = (Button)ResourceLoader.Load<PackedScene>("res://Scenes/action.tscn").Instantiate();

        foreach (var child in vbox.GetChildren())
        {
            child.QueueFree();
        }


        if (zoomed.placerId != ServerManager.client.id) return;
        if (currentPhase != 2 || currentTurn != ServerManager.client.id) return;

        Type type = null;

        try
        {
            type = Type.GetType("card." + card.card.Name.Replace(' ', '_'));
        }
        catch (Exception e)
        {
            GD.Print(e);
            return;
        }

        foreach (var t in GetAllTypes(type))
        {
            Action cardClass = (Action)Activator.CreateInstance(t);

            if (zoomed.status.Contains(cardClass.name)) continue;

            Button action = (Button)actionButton.Duplicate();

            action.Text = cardClass.name;

            action.Pressed += () =>
            {
                cardClass.Run(card);
                ShowActions(card);
            };

            vbox.CallDeferred(Node.MethodName.AddChild, action);
        }
    }

    public static List<Type> GetAllTypes(Type type)
    {
        List<Type> types = new();

        types.AddRange(type.GetNestedTypes());
        types.AddRange(type.BaseType.GetNestedTypes());

        return types;
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

    public static void ChooseView()
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

    public static void UpdateEffects()
    {
        foreach (var card in sceneTree.GetChildren())
        {
            if (card is not Card) continue;

            Card c = (Card)card;

            c.status.Clear();
            c.effects.Clear();

            c.status.AddRange(c.tempStatus);
            c.effects = c.tempEffects;

            c.tempEffects.Clear();
            c.tempStatus.Clear();
        }
    }

    public static void AddEffect(EUP eup)
    {
        foreach (var card in sceneTree.GetChildren())
        {
            if (card is not Card) continue;
            Card c = (Card)card;
            if (c.placerId == eup.targetId && c.card.Id == eup.card.Id && eup.slot == (c.slot))
            {
                c.AddEffect(eup.name, eup.param);
                return;
            }
        }
    }

    public static void AddStatus(EUP eup)
    {
        foreach (var card in sceneTree.GetChildren())
        {
            if (card is not Card) continue;
            Card c = (Card)card;
            if (c.placerId == eup.targetId && c.card.Id == eup.card.Id && eup.slot == (c.slot))
            {
                c.AddStatus(eup.name);
                break;
            }
        }

        if (zoomed != null) ShowActions(zoomed);
    }

    public static void UpdateTurn()
    {
        Label label = sceneTree.GetNode<Label>("CanvasLayer/Control/Phase");

        if (currentTurn != ServerManager.client.id)
        {
            label.Text = "Opponent's Turn";
            sceneTree.GetNode<DirectionalLight3D>("DirectionalLight3D").LightColor = new Color(1, 1, 1);
            sceneTree.GetNode<Button>("CanvasLayer/Control/EndTurn").Disabled = true;
            return;
        }
        else sceneTree.GetNode<Button>("CanvasLayer/Control/EndTurn").Disabled = false;

        switch (currentPhase)
        {
            case 0:
                label.Text = "Draw Phase";
                break;
            case 1:
                label.Text = "Placing Phase";
                break;
            case 2:
                label.Text = "Attack Phase";
                sceneTree.GetNode<DirectionalLight3D>("DirectionalLight3D").LightColor = new Color(1, (float)0.8, (float)0.8);
                break;
        }
    }

    public static void UpdatePlayer(PUP pup)
    {
        int playerNum = pup.player.id != ServerManager.client.id ? 2 : 1;

        Label hpLabel = sceneTree.GetNode<Label>("CanvasLayer/Control/Player" + playerNum + "Pic/HP");
        hpLabel.Text = pup.player.lifePoints + " Resolve";
    }

    public static void GetViableCard(string typeFilter)
    {
        ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Show();
        GridContainer container = (GridContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands/GridContainer");

        ClearTempHand();

        foreach (var card in ServerManager.client.hand)
        {
            if (card.Type != typeFilter) continue;
            if (chosen.Contains(card)) continue;
            if (cardSummon.ContainsKey(card)) continue;

            var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/2d_card.tscn").Instantiate().Duplicate();

            container.AddChild(thescene);

            D2Card c = thescene as D2Card;

            c.setCard(card);
        }

        HandShown = true;
    }

    public static void ChooseSacrifice(CardObject card, int slot)
    {
        ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Show();
        GridContainer container = (GridContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands/GridContainer");

        ClearTempHand();

        if (sacrificialAmt >= matchSacrifice)
        {
            ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = card, action = "place", targetSlot = slot, param = sacrificed }));
            toSummon.Clear();
            ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Hide();
            HandShown = false;
            return;
        }

        foreach (var curCard in cards)
        {
            if (curCard.placerId != ServerManager.client.id) continue;
            if (sacrificed.Contains(curCard.slot)) continue;
            if (curCard.card.Class == "Spell") continue;

            var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/2d_card.tscn").Instantiate().Duplicate();

            container.AddChild(thescene);

            D2Card c = thescene as D2Card;

            c.setCard(curCard.card);
            c.slot = curCard.slot;
        }

        HandShown = true;
    }

    public static void CancelActions()
    {
        if (summonCard != null)
        {
            ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Hide();
            HandShown = false;

            summonCard.skip = true;
            summonCard.Summon(cardSummon.First().Key, cardSummon.First().Value);

            summonCard = null;
            cardSummon = new();
            chosen = new();

            return;
        }

        summonCard = null;
        cardSummon = new();
        chosen = new();

        chooseTarget = null;
        targetPlace = null;
        selectedHand = null;

        if (zoomed != null) ReturnView(zoomed);

        toSummon.Clear();
        matchSacrifice = 0;
        sacrificed.Clear();
        sacrificialAmt = 0;

        ((Button)sceneTree.GetNode("/root/Game/CanvasLayer/Control/Hand")).Disabled = false;
        ((Container)sceneTree.GetNode("/root/Game/CanvasLayer/Control/PlayerHand")).Hide();
        ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Hide();

        HandShown = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel")) CancelActions();

        if (@event.IsActionPressed("Left_Click") && !@event.IsEcho())
            if (HandShown)
            {
                HandInput();
                ((Control)sceneTree.GetNode("/root/Game/CanvasLayer/Control")).AcceptEvent();
            }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Check if the player clicked
        if (@event.IsActionPressed("Left_Click") && !@event.IsEcho())
        {
            if (chooseTarget != null)
            {
                ChooseTargetInput(@event);
                return;
            }

            Card c = null;
            bool skip = false;

            // Check each card to see if there was one that was clicked
            foreach (Card card in cards)
            {
                if (targetPlace != null) break;

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
            }
            else if (c == null && !skip && currentTurn == ServerManager.client.id)
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

                if (cardObject.Class == "Spell")
                {
                    if (!collider.Name.ToString().Contains(cardObject.Class.ToString())) { return; }
                }
                else if (collider.Name.ToString().Contains("Spell")) { return; }

                int slot;

                try
                {
                    slot = int.Parse(Regex.Match(collider.Name, @"\d+").Value)-1;
                }
                catch (Exception)
                {
                    return;
                }

                if (targetPlace == null)
                {
                    if (cardObject.Class == "Spell") ServerManager.client.WriteStream(PacketManager.ToJson(new CAP { placerId = ServerManager.client.id, card = cardObject, action = "place", targetSlot = slot }));
                    else
                    {
                        BaseCard action = (BaseCard)Activator.CreateInstance(Type.GetType("card." + cardObject.Name.Replace(' ', '_')));
                        action.Summon(cardObject.Clone(), slot);
                    }
                }
                else
                {
                    targetPlace.Run(zoomed, slot);
                }
            }
        }
    }

    public override void _Ready()
    {
        Button buttonCamera = (Button)GetNode("/root/Game/CanvasLayer/Control/ChangeView");
        Button buttonHand = (Button)GetNode("/root/Game/CanvasLayer/Control/Hand");
        Button buttonTurn = GetNode<Button>("/root/Game/CanvasLayer/Control/EndTurn");
        description = (RichTextLabel)GetNode("/root/Game/CanvasLayer/Control/Desc");
        var hand = (Container)GetNode("/root/Game/CanvasLayer/Control/PlayerHand");
        sceneTree = this;

        curCamera = new Camera3D();

        AddChild(curCamera);

        curCamera.GlobalPosition = cam1Pos;
        curCamera.GlobalRotationDegrees = cam1Rot;

        curCamera.MakeCurrent();
        //-----------------------//
        for (int i = 0; i < 20; i++)
        {
            var thescene2 = (Node3D)ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate().Duplicate();


            var cardGlobalPosition2 = sceneTree.GetNode<MeshInstance3D>("Player1/Deck").GlobalPosition;

            sceneTree.CallDeferred(Node.MethodName.AddChild, thescene2);

            thescene2.Position = new Vector3(cardGlobalPosition2.X, (0.005f*(i*3)), cardGlobalPosition2.Z);
            thescene2.RotationDegrees = new Vector3(0, 0, 180);
            var thescene1 = (Node3D)ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate().Duplicate();


            var cardGlobalPosition1 = sceneTree.GetNode<MeshInstance3D>("Player2/Deck").GlobalPosition;

            sceneTree.CallDeferred(Node.MethodName.AddChild, thescene1);

            thescene1.Position = new Vector3(cardGlobalPosition1.X, (0.005f * (i * 3)), cardGlobalPosition1.Z);
            thescene1.RotationDegrees = new Vector3(0, 180, 180);
        }
        //-----------------------//

        // Move between the two camera angles
        buttonCamera.ButtonDown += () =>
        {
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
            hand.Show();
            HandShown = true;
            buttonHand.Disabled = true;
        };
        buttonTurn.ButtonDown += () =>
        {
            ServerManager.client.WriteStream(PacketManager.ToJson(new GSP()));
        };
    }

    public override void _Process(double delta)
    {
        if (changeScene)
        {
            changeScene = false;
            Main.inGame = false;
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
                case "fadd":
                    forgotten.Add(cap.card);
                    break;
                case "uadd":
                    unforgotten.Add(cap.card);
                    break;
                case "urem":
                    unforgotten.Remove(cap.card);
                    break;
                case "ueffects":
                    UpdateEffects();
                    break;
                case "uturn":
                    UpdateTurn();
                    break;
            }

            actionQueue.RemoveAt(0);
        }

        while (playerQueue.Count > 0)
        {
            var pup = playerQueue[0];

            UpdatePlayer(pup);

            playerQueue.RemoveAt(0);
        }

        while (effectQueue.Count > 0)
        {
            var eup = effectQueue[0];

            switch (eup.type)
            {
                case "effect":
                    AddEffect(eup);
                    break;
                case "status":
                    AddStatus(eup);
                    break;
            }

            effectQueue.RemoveAt(0);
        }

        if (winner != null)
        {
            ((Panel)sceneTree.GetNode("/root/Game/CanvasLayer/Control/EndScreen")).Show();
            ((Label)sceneTree.GetNode("/root/Game/CanvasLayer/Control/EndScreen/Label")).Text = ServerManager.client.account.username == winner ? "Congratulations!\nYou Won!" : "Uh-oh,\nYou Lose";
            winner = null;
        }

        base._Process(delta);
    }

    public static void HandInput()
    {
        Button buttonHand = (Button)sceneTree.GetNode("/root/Game/CanvasLayer/Control/Hand");
        var hand = (Container)sceneTree.GetNode("/root/Game/CanvasLayer/Control/PlayerHand");

        if (selectedHand != null)
        {
            selectedHand.keepShown = true;

            if (lastSelectedHand == selectedHand)
            {
                cardObject = selectedHand.card;

                selectedHand = null;
                lastSelectedHand.keepShown = false;
                lastSelectedHand.ReturnCard();

                if (targetPlace == null && toSummon.Count <= 0)
                {
                    hand.Hide();
                    buttonHand.Disabled = false;
                }
                else
                {
                    ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Hide();
                }

                HandShown = false;

                if (toSummon.Count > 0)
                {
                    sacrificed.Add(lastSelectedHand.slot);
                    sacrificialAmt += cardObject.SacrificialValue;

                    ChooseSacrifice(toSummon.First().Key, toSummon.First().Value);

                    lastSelectedHand = null;
                }

                if (summonCard != null)
                {
                    chosen.Add(cardObject);

                    summonCard.Summon(cardSummon.First().Key, cardSummon.First().Value);

                    lastSelectedHand = null;
                }

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

            if (targetPlace == null)
            {
                hand.Hide();
                buttonHand.Disabled = false;
            }
            else
            {
                ((ScrollContainer)sceneTree.GetNode("CanvasLayer/Control/SelectionHands")).Hide();
            }

            HandShown = false;

            return;

        }
    }

    public static void ChooseTargetInput(InputEvent @event)
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
            if (occupied.card.Class.Contains("Spell")) return;

            chooseTarget.Run(zoomed, occupied.slot);
            chooseTarget = null;
            ReturnView(zoomed);
        }

        return;
    }
}
