using System;
using card;

namespace player
{
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
		public Dictionary<packets.CAP, int> unforgotten;
		public List<string> forgotten;

		public TempCard[] fieldRowOne;
		public TempCard[] fieldRowTwo;

		public Player(int _id)
		{
			id = _id;

			lifePoints = 10000;

			fieldRowOne = new TempCard[5];
			fieldRowTwo = new TempCard[5];

			unforgotten = new();
			forgotten = new();
		}

		public Player()
		{

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
			p.fieldRowOne = null;
			p.fieldRowTwo = null;
			p.forgotten = null;
			p.unforgotten = null;
			p.hand = null;

			return p;
		}
	}
}