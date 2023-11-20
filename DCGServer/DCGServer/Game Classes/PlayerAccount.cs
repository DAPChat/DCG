using MongoDB.Bson;
using System;

public class PlayerAccount
{
	public ObjectId id;

	public string username;
	public string password;
	public int rank;
	public int streak;

	public string cardBack;
	public string profilePicture;

	public PlayerAccount(string _username, string _password)
	{
		username = _username;
		password = _password;
	}
}