using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

public class Database
{
	private static readonly string connectionUri = "mongodb+srv://thedcg:II7oVRzWUQmjJN5c@cluster0.bcztjvi.mongodb.net/?retryWrites=true&w=majority";

	static MongoClient client = null;

	// Connect to the database
	public static void Connect()
	{
		if (client == null) 
			client = new MongoClient(connectionUri);

		var result = client.GetDatabase("DCG").RunCommand<BsonDocument>(new BsonDocument("ping", 1));

        Console.WriteLine("Connected to Database!");

		ResetAccounts();
	}

	// Get a specific card from database
	public static Card GetCard(string cardId)
	{
		if (client == null) 
			Connect();

		var collection = client.GetDatabase("DCG").GetCollection<Card>("Cards");

        var filter = Builders<Card>.Filter.Eq(Card => Card.Id, ObjectId.Parse(cardId));

        var result = collection.Find(filter).FirstOrDefault();

		return result;
	}

	// Check if the username is available
	public static bool CheckAvailableAcc(string username)
	{
		var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

		var filter = Builders<PlayerAccount>.Filter.Eq(PlayerAccount => PlayerAccount.username, username);

		var result = collection.Find(filter).ToList();

		if (result.Count != 0)
		{
			return false;
		}

		return true;
	}

	// Adds an account to the database
	public static PlayerAccount AddAcc(ACP account, Client _client)
	{
		string username = account.username;
		string password = account.password;

		SHA256 hash = SHA256.Create();

		// Hashes the password for security purposes
		password = Encoding.UTF8.GetString(hash.ComputeHash(Encoding.UTF8.GetBytes(password)));

        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

		PlayerAccount newAccount = new(username, password);

		// Ensures two players cannot be logged into the same account at once
		newAccount.loggedIn = true;

        collection.InsertOne(newAccount);

        Console.WriteLine("Created new account with the username {0}", username);

		return VerifyAcc(account, _client);
    }

	// Verifies if the username and password are correct
	public static PlayerAccount VerifyAcc(ACP account, Client _client)
	{
		string username = account.username;
		string password = account.password;

        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

        var filter = Builders<PlayerAccount>.Filter.Eq(PlayerAccount => PlayerAccount.username, username);

        var result = collection.Find(filter).ToList();

		if (result.Count == 0)
		{
			account.error = "Account does not exist";
			return null;
		}

		if (result.First().username == username)
		{
			password = Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));

			if (result.First().password == password)
			{
				if (result.First().loggedIn)
				{
					account.error = "This account is already in use elsewhere";
					return null;
				}

				Console.WriteLine("User, {0}, successfully logged in!", username);
				_client.Login(result.First());

                // Ensures two players cannot be logged into the same account at once
                var update = Builders<PlayerAccount>.Update.Set(PlayerAccount => PlayerAccount.loggedIn, true);
				var updateResult = collection.UpdateOne(filter, update);

				return result.First();
			}
			else account.error = "Incorrect password";
		}

		return null;
    }

	// Opens up the account to be able to log in from another place
	public static void Logout(PlayerAccount account)
	{
        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

        var filter = Builders<PlayerAccount>.Filter.Eq(PlayerAccount => PlayerAccount.username, account.username);

        var update = Builders<PlayerAccount>.Update.Set(PlayerAccount => PlayerAccount.loggedIn, false);

        var updateResult = collection.UpdateOne(filter, update);
    }

	// Returns a list of all card ids
	public static List<string> CardIds()
	{
        if (client == null)
            Connect();

        var collection = client.GetDatabase("DCG").GetCollection<Card>("Cards");

        var filter = Builders<Card>.Filter.Empty;

        var result = collection.Find(filter).ToList();

		List<string> cardIds = new();

		foreach (Card card in result)
		{
			cardIds.Add(card.Id.ToString());
		}

		return cardIds;
    }

	// Effectively sets all accounts to be logged out of (if the server closes while a client is active)
	private static void ResetAccounts()
	{
        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

        var filter = Builders<PlayerAccount>.Filter.Eq(PlayerAccount => PlayerAccount.loggedIn, true);

        var result = collection.Find(filter).ToList();

        var update = Builders<PlayerAccount>.Update.Set(PlayerAccount => PlayerAccount.loggedIn, false);

        foreach ( PlayerAccount account in result )
		{
			collection.UpdateMany(filter, update);
		}
    }
}