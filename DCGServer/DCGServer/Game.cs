using System;
using System.Text;
using System.Text.Json;

public class Game
{
	public int id;

	// Store the clients in the game with their id
	public Dictionary<int, Client> clients = new Dictionary<int, Client>();
	public List<int> clientIds = new List<int>();

	GameBoard currentBoard;

	public Game(int _id)
	{
		id = _id;
	}

	public Game(int _id, List<Client> _clients)
	{
		// Ensures only 2 players can be in each game
		// Re-adds the clients to the queue to give them a second-chance
		if(_clients.Count > 2)
		{
            Console.WriteLine("Error Creating Match!");
			foreach(Client _client in _clients)
			{
				Server.Queue(_client.id, _client);
			}
            return;
        }

		id = _id;

		// Add each client and update the client's data
		foreach (Client _client in _clients)
		{
			int currentClientId = _client.id;
			clients.Add(currentClientId, _client);

			clientIds.Add(currentClientId);

			Console.Write(id + ": ");
			Console.WriteLine("Client Added! With ID {0} on server {1}", currentClientId, id);

			_client.tcp.WriteStream(IntToByte(_id));
			_client.gameId = id;
		}


         currentBoard = new GameBoard(this);
    }

	byte[] IntToByte(int _msg)
	{
		// Convert an integer to a byte
		return Encoding.ASCII.GetBytes(_msg.ToString());
	}

	// Shouldn't be used... just for testing right now and opens for future concepts
	public void AddClient(Client _client)
	{
		if(clients.Count >= 2)
		{
			Console.WriteLine("Error Joining Match!");
			Server.Queue(_client.id, _client);
			return;
		}

		clients.Add(_client.id, _client);

		_client.tcp.WriteStream(IntToByte(id));
		_client.gameId = id;
	}

	// Remove the specified client from the game
	// Add the remaining client to the queue
	public void Disconnect(int _id)
	{
		clients[_id].Disconnect();
		clients.Remove(_id);

		clients.First().Value.gameId = 0;

		Server.Queue(clients.First().Key, clients.First().Value);
	}

	class GameBoard
	{
		bool gameState;
		int turn;
		int phase;
		int round;

		string image;

		Player player1;
		Player player2;

		public GameBoard(Game _game)
		{
			gameState = true;
			turn = 0;
			phase = 0;
			round = 1;

			image = null;

			player1 = new Player(_game.clientIds[0]);
			player2 = new Player(_game.clientIds[1]);
		}
	}
}