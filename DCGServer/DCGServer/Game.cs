using System;
using System.Text;

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

        int currentClientId = _client.id;

        _client.tcp.WriteStream(ToByte(id));
		_client.gameId = id;

		clients.Add(currentClientId, _client);
	}

	public Game(int _id, List<Client> _clients)
	{
		id = _id;

		foreach (Client _client in _clients)
		{
			int currentClientId = _client.id;
			clients.Add(currentClientId, _client);

			Console.Write(id + ": ");
			Console.WriteLine("Client Added! With ID {0} on server {1}", currentClientId, id);

			_client.tcp.WriteStream(ToByte(_id));
			_client.gameId = id;
		}
	}

	byte[] ToByte(int _msg)
	{
		return Encoding.ASCII.GetBytes(_msg.ToString());
	}

	public void AddClient(Client _client)
	{
		clients.Add(_client.id, _client);

		_client.tcp.WriteStream(ToByte(id));
		_client.gameId = id;
	}

	public void Disconnect(int _id)
	{
		clients[_id].Disconnect();
		clients.Remove(_id);

		clients.First().Value.gameId = 0;

		Server.Queue(clients.First().Key, clients.First().Value);
	}
}