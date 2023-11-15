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
		Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
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
}