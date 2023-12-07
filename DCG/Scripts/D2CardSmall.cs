using Godot;
using System;
using System.Drawing;

public partial class D2CardSmall : Control
{
    public GameScene.CardObject card;
    public RichTextLabel description;
    static Button ChangeBtn;
    static Button RemoveBtn;

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
            GetNode<Control>("Options").Show();
        };

        GetNode<Area2D>("FrontFace/Area2D").MouseExited += () =>
        {
            GetNode<Control>("Options").Hide();
        };
    }
    public void ChangeCard()
    {
        // change the grid from deck to all cards in deck, change the label at top, and finally disable the hover on cards while enabling click of cards (for replacement)
    }
    public void RemoveCard()
    {
        // simply remove the card from the current deck
    }
    public void ReplaceCard()
    {
        //change grid back to the deck grid, change label, reenable the hover of cards, disable clicking of cards 
    }

}
