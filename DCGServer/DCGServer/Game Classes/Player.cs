using System;

public class Player
{
	public int id;
	public string username;
	public int rank;
	public int streak;

	public int playerNum;

	public string cardBack;
	public string profilePicture;

	public int lifePoints;
	public int timer;

	public List<string> deck;
	public List<string> hand;
	public List<string> unforgotten;
	public List<string> forgotten;

	public List<string> fieldRowOne;
	public List<string> fieldRowTwo;

	public Player(int _id) 
	{ 
		id = _id;
	}

	public Player Clone()
	{
        Player p = new(id)
        {
            username = username,
            rank = rank,
            streak = streak,
            playerNum = playerNum,
            cardBack = cardBack,
            profilePicture = profilePicture,
            lifePoints = lifePoints,
            timer = timer,
            hand = hand,
            unforgotten = unforgotten,
            forgotten = forgotten,
            fieldRowOne = fieldRowOne,
            fieldRowTwo = fieldRowTwo,
            deck = deck
        };

        return p;
    }

    public Player Client()
	{
		Player p = this.Clone();

		p.deck = null;

        return p;
	}
}