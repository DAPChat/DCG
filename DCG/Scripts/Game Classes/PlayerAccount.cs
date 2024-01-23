using System;

namespace player
{
	public class PlayerAccount
	{
		public string id;

		public string username;
		public string password;

		public int rank;
		public double points;
		public int streak;
		public int totalWins;
		public int totalLosses;

		public string cardBack;
		public string profilePicture;

		public string[] cardBacks;

		public int[] cards;
		public int[][] decks;

		public PlayerAccount(string _username, string _password)
		{
			username = _username;
			password = _password;

			decks = new int[8][];
		}
	}
}