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
}