using System;
using System.Collections.Generic;

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
        public Dictionary<string, int> unforgotten;
        public List<string> forgotten;

		public string[] fieldRowOne;
		public string[] fieldRowTwo;

		public Player(int _id)
		{
			id = _id;

			fieldRowOne = new string[5];
			fieldRowTwo = new string[5];
		}

		public Player()
		{
			fieldRowOne = new string[5];
			fieldRowTwo = new string[5];
		}
	}
}