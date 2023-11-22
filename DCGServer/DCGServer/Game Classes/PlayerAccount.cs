﻿using MongoDB.Bson;
using System;

public class PlayerAccount
{
	public ObjectId id;

	public string username;
	public string password;

    public bool loggedIn;

    public int rank;
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

		decks = new int[3][];
	}
}