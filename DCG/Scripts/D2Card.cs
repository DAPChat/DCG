using Godot;
using System;

public partial class D2Card : Node2D
{
    public GameScene.CardObject card;
    public Label description;
    public void setCard(GameScene.CardObject _card, Vector2 pos) //add added child card
    {
        card = _card;

        description = GetNode<Label>("CanvasLayer/FrontFace/BottomCard/Description");

        GetNode<Label>("CanvasLayer/FrontFace/TopCard/Name").Text = card.Name.ToString();
        GetNode<Label>("CanvasLayer/FrontFace/TopCard/Rank").Text = card.Rank.ToString();
        description.Text = card.Description.Length > 133 ? card.Description.ToString().Substr(0, 130) + "..." : card.Description.ToString();

        string statsText = "";

        if (card.Atk > 0) statsText += card.Atk.ToString() + " ATK";
        if (card.Mana > 0 || card.Hp > 0) statsText += " / ";
        if (card.Hp > 0) statsText += card.Hp.ToString() + " HP";
        if (card.Mana > 0) statsText += " / ";
        if (card.Mana > 0) statsText += card.Mana.ToString() + " Mana";

        GetNode<Label>("CanvasLayer/FrontFace/BottomCard/Stats").Text = statsText;

        Position = pos;
        Position = new Vector2(Position.X, Position.Y);
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

            var mesh = GetNode<TextureRect>("CanvasLayer/FrontFace/MiddleCard");
            mesh.Texture = ImageTexture.CreateFromImage(img); //might not work

            Show();
        };
        Error error = request.Request(url);
    }
}
