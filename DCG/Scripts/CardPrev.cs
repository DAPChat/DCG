using Godot;
using System;

public partial class CardPrev : ColorRect
{
	public static GameScene.CardObject card = null;

	private Label name;
	private Label rank;
	private RichTextLabel description;
	private Label stats;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();

		name = (Label)GetNode("Name");
		rank = (Label)GetNode("Rank");
		stats = (Label)GetNode("Stats");
		description = (RichTextLabel)GetNode("Description");
	}

    public override void _Process(double delta)
    {
        if (card != null)
		{
			Show();

			name.Text = card.Name;
			rank.Text = card.Rank;
			description.Text = card.Description;

			stats.Text = $"{card.Atk} ATK / {card.Hp} HP";

        }
		else
		{
			Hide();
		}
    }
}
