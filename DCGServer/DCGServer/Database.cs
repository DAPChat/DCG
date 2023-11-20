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


	public static byte[] GetCard(int i)
	{
		if (client == null) 
			Connect();

		var collection = client.GetDatabase("DCG").GetCollection<Card>("Cards");

		var filter = Builders<Card>.Filter.Empty;

		var result = collection.Find(filter).ToList();

		Card card = result[i];

		return PacketManager.ToJson(card);
	}

	public static bool CheckAvailableAcc(string username)
	{
		var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

		var filter = Builders<PlayerAccount>.Filter.Eq(PlayerAccount => PlayerAccount.username, username);

		var result = collection.Find(filter).ToList();

		if (result.Count != 0) return false;

		return true;
	}

	public static void AddAcc(string username, string password)
	{
        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

        collection.InsertOne(new PlayerAccount(username, password));

		Console.WriteLine("Created new account with the username {0}", username);
    }

	public static bool VerifyAcc(string username, string password)
	{
        var collection = client.GetDatabase("DCG").GetCollection<PlayerAccount>("Players");

        var filter = Builders<PlayerAccount>.Filter.Eq(PlayerAccount => PlayerAccount.username, username);

        var result = collection.Find(filter).ToList();

		if (result.Count == 0) return false;

		if (result.First().username == username)
		{
			if(result.First().password == password)
			{
				Console.WriteLine("User, {0}, successfully logged in!", username);
				return true;
			}
		}

		return false;
    }
}