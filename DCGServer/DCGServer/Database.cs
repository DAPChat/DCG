using MongoDB.Bson;
using MongoDB.Driver;

public class Database
{
	protected private static readonly string connectionUri = "mongodb+srv://thedcg:II7oVRzWUQmjJN5c@cluster0.bcztjvi.mongodb.net/?retryWrites=true&w=majority";

	static MongoClient client = null;

	public static void Connect()
	{
		if (client == null) 
			client = new MongoClient(connectionUri);

		var result = client.GetDatabase("DCG").RunCommand<BsonDocument>(new BsonDocument("ping", 1));

        Console.WriteLine("Connected to Database!");
	}


	public static Card GetCard(int i)
	{
		if (client == null) 
			Connect();

		var collection = client.GetDatabase("DCG").GetCollection<Card>("Cards");

		var filter = Builders<Card>.Filter.Empty;

		var result = collection.Find(filter).ToList();

		Card card = result[i];

		return card;
	}

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

	public static PlayerAccount AddAcc(ACP account, Client _client)
	{
		string username = account.username;
		string password = account.password;

        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

		PlayerAccount newAccount = new(username, password);

        collection.InsertOne(newAccount);

        Console.WriteLine("Created new account with the username {0}", username);

		return VerifyAcc(account, _client);
    }

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
			if (result.First().password == password)
			{
				if (result.First().loggedIn)
				{
					account.error = "This account is already in use elsewhere";
					return null;
				}

				Console.WriteLine("User, {0}, successfully logged in!", username);
				_client.Login(result.First());

				var update = Builders<PlayerAccount>.Update.Set(PlayerAccount => PlayerAccount.loggedIn, true);
				var updateResult = collection.UpdateOne(filter, update);

				return result.First();
			}
			else account.error = "Incorrect password";
		}

		return null;
    }

	public static void Logout(PlayerAccount account)
	{
        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

        var filter = Builders<PlayerAccount>.Filter.Eq(PlayerAccount => PlayerAccount.username, account.username);

        var update = Builders<PlayerAccount>.Update.Set(PlayerAccount => PlayerAccount.loggedIn, false);

        var updateResult = collection.UpdateOne(filter, update);
    }
}