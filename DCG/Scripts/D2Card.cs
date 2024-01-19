using Godot;
using System;

public partial class D2Card : Control
{
    public GameScene.CardObject card;
    public RichTextLabel description;

    Vector2 startPos = new Vector2();

    float gridSize = 225;

    public bool hovered = false;
    public bool keepShown = false;

    public int slot;

    // Works similarly to the Card class
    public void setCard(GameScene.CardObject _card) //add added child card
    {
        card = _card;

        description = GetNode<RichTextLabel>("FrontFace/BottomCard/Description");

        Label name = GetNode<Label>("FrontFace/TopCard/Name");

        Font font = (Font)name.GetThemeFont("font").Duplicate();

        name.AddThemeFontOverride("font", font);

        var size = 15;

        while (font.GetStringSize(name.Text, name.HorizontalAlignment, -1, size).X > 158)
        {
            size -= 1;
        }

        name.AddThemeFontSizeOverride("font_size", size);

        name.Text = card.Name.ToString();

        GetNode<Label>("FrontFace/TopCard/Rank").Text = card.Rank.ToString();
        description.Text = card.SacrificialValue + " - " + card.Description.ToString();

        string statsText = "";

        if (card.Atk > 0) statsText += card.Atk.ToString() + " ATK";
        if (card.Mana > 0 || card.Hp > 0) statsText += " / ";
        if (card.Hp > 0) statsText += card.Hp.ToString() + " HP";
        if (card.Mana > 0) statsText += " / ";
        if (card.Mana > 0) statsText += card.Mana.ToString() + " Mana";

        GetNode<Label>("FrontFace/BottomCard/Stats").Text = statsText;

        if (IsInsideTree())
            getImg(_card.Img.ToString());
        else
            TreeEntered += () =>
            {
                getImg(_card.Img.ToString());
            };
    }

    private void getImg(string url)
    {
        url = "https://dapchat.github.io/DCGimages/" + url;
        HttpRequest request = new HttpRequest();
        AddChild(request);
        Image img = new Image();
        request.RequestCompleted += (_result, responsecode, header, body) =>
        {
            try
            {
                Error error = img.LoadJpgFromBuffer(body);

                if (error != Error.Ok)
                {
                    GameScene.images.Add(card.Id.ToString(), null);
                    Show();
                    return;
                }
            }
            catch (Exception) { Show(); return; }

            var mesh = GetNode<TextureRect>("FrontFace/Container/MiddleCard");

            ImageTexture texture = ImageTexture.CreateFromImage(img);

            if (!GameScene.images.ContainsKey(card.Id.ToString()))
                GameScene.images.Add(card.Id.ToString(), texture);

            mesh.Texture = texture; //might not work

            Show();
        };
        if (!GameScene.images.ContainsKey(card.Id.ToString()))
        {
            Error error = request.Request(url);
        }
        else
        {
            var mesh = GetNode<TextureRect>("FrontFace/Container/MiddleCard");

            ImageTexture texture = GameScene.images[card.Id.ToString()];

            mesh.Texture = texture;
        }
    }

    public override void _Ready()
    {
        GetNode<Area2D>("FrontFace/Area2D").MouseEntered += () =>
        {
            if (!hovered) startPos = Position;

            GameScene.selectedHand = this;

            hovered = true;
            ZIndex = 1;
            Tween tween = CreateTween();
            tween.TweenProperty(GetNode<TextureRect>("FrontFace"), "position:y", startPos.Y - 100, .25);
        };

        GetNode<Area2D>("FrontFace/Area2D").MouseExited += () =>
        {
            if (hovered && GameScene.selectedHand == this) GameScene.selectedHand = null;

            if (keepShown) return;

            ReturnCard();
        };
    }

    public void ReturnCard()
    {
        ZIndex = 0;
        Tween tween = CreateTween();

        hovered = false;

        tween.TweenProperty(GetNode<TextureRect>("FrontFace"), "position:y", startPos.Y, .25);
    }

    public override void _Process(double delta)
    {
        if (gridSize != GameScene.gridSeparation)
        {
            gridSize = GameScene.gridSeparation;

            CollisionShape2D collider = GetNode<CollisionShape2D>("FrontFace/Area2D/CollisionShape2D");
            RectangleShape2D colliderShape = (RectangleShape2D)collider.Shape;

            colliderShape.Size = new Vector2((float)Math.Clamp(gridSize, 10, 225), colliderShape.Size.Y);

            collider.Position = new Vector2((float)(Math.Clamp(gridSize, 10, 225)/2), collider.Position.Y);
        }
    }
}
