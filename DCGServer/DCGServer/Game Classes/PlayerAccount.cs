﻿using MongoDB.Bson;
using System;

namespace player
{
	public class PlayerAccount
	{
		public ObjectId id;

		public string username;
		public string password;

		public bool loggedIn;

		public int rank;
		public double points;
		public int streak;
		public int totalWins;
		public int totalLosses;

		public string cardBack;
		public string profilePicture;

		public string[] cardBacks;

		public int curDeck;

		public string[] cards;
		public string[][] decks;

		public PlayerAccount(string _username, string _password)
		{
			username = _username;
			password = _password;

			decks = new string[3][];
		}
	}
}