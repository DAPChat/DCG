using System;

public class Game
{
	public int id;
	public Dictionary<int, Client> clients = new Dictionary<int, Client>();

	public Game(int _id)
	{
		id = _id;
	}

	public Game(int _id, Client _client)
	{
		id = _id;

		int currentClientId = clients.Count + 1;

		clients.Add(currentClientId, _client);
	}

	public Game(int _id, Client[] _clients)
	{
		id = _id;

		foreach (Client _client in _clients)
		{
			int currentClientId = clients.Count + 1;
			clients.Add(currentClientId, _client);
		}
	}
}