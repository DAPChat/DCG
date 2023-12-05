using Godot;
using System;

public partial class D2Card : Control
{
    public GameScene.CardObject card;
    public RichTextLabel description;

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

            var mesh = GetNode<TextureRect>("FrontFace/MiddleCard");
            mesh.Texture = ImageTexture.CreateFromImage(img); //might not work

            Show();
        };
        Error error = request.Request(url);
    }
}
