using System;

public class Player
{
	public int id;
	public string username;
	public int rank;
	public int streak;

	public string cardBack;
	public string profilePicture;

	public int lifePoints;
	public int timer;

	public List<Card> deck;
	public List<Card> hand;
	public List<Card> unforgotten;
	public List<Card> forgotten;

	public List<Card> fieldRowOne;
	public List<Card> fieldRowTwo;

	public Player(int _id) 
	{ 
		id = _id;
	}
}