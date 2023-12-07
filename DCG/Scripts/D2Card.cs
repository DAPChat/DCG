using Godot;
using System;
using System.Drawing;

public partial class D2Card : Control
{
    public GameScene.CardObject card;
    public RichTextLabel description;

    int gridSize = 225;

    bool hovered = false;

    // Works similarly to the Card class
    public void setCard(GameScene.CardObject _card) //add added child card
    {
        card = _card;

        description = GetNode<RichTextLabel>("FrontFace/BottomCard/Description");

        GetNode<Label>("FrontFace/TopCard/Name").Text = card.Name.ToString();
        GetNode<Label>("FrontFace/TopCard/Rank").Text = card.Rank.ToString();
        description.Text = card.Description.ToString();

        string statsText = "";

        if (card.Atk > 0) statsText += card.Atk.ToString() + " ATK";
        if (card.Mana > 0 || card.Hp > 0) statsText += " / ";
        if (card.Hp > 0) statsText += card.Hp.ToString() + " HP";
        if (card.Mana > 0) statsText += " / ";
        if (card.Mana > 0) statsText += card.Mana.ToString() + " Mana";

        GetNode<Label>("FrontFace/BottomCard/Stats").Text = statsText;

        TreeEntered += () =>
        {
            getImg(_card.Img.ToString());
        };
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

            var mesh = GetNode<TextureRect>("FrontFace/Container/MiddleCard");

            ImageTexture texture = ImageTexture.CreateFromImage(img);

            if (!GameScene.images.ContainsKey(card.Id))
                GameScene.images.Add(card.Id, texture);

            mesh.Texture = texture; //might not work

            Show();
        };
        if (!GameScene.images.ContainsKey(card.Id))
        {
            Error error = request.Request(url);
        }
        else
        {
            var mesh = GetNode<TextureRect>("FrontFace/MiddleCard");

            ImageTexture texture = GameScene.images[card.Id];

            mesh.Texture = texture; //might not work
        }
    }

    public override void _Ready()
    {
        Vector2 startPos = new Vector2();
        GetNode<Area2D>("FrontFace/Area2D").MouseEntered += () =>
        {
            if (!hovered) startPos = Position;

            hovered = true;
            ZIndex = 1;
            Tween tween = CreateTween();
            tween.TweenProperty(GetNode<TextureRect>("FrontFace"), "position:y", startPos.Y - 100, .25);
        };

        GetNode<Area2D>("FrontFace/Area2D").MouseExited += () =>
        {
            ZIndex = 0;
            Tween tween = CreateTween();
            tween.Finished += () =>
            {
                hovered = false;
            };
            tween.TweenProperty(GetNode<TextureRect>("FrontFace"), "position:y", startPos.Y, .25);
        };
    }

    public override void _Process(double delta)
    {
        if (gridSize != GameScene.gridSeparation)
        {
            gridSize = GameScene.gridSeparation;

            CollisionShape2D collider = GetNode<CollisionShape2D>("FrontFace/Area2D/CollisionShape2D");
            RectangleShape2D colliderShape = (RectangleShape2D)collider.Shape;

            colliderShape.Size = new Vector2(gridSize, colliderShape.Size.Y);

            collider.Position = new Vector2(gridSize/2, collider.Position.Y);
        }
    }
}
