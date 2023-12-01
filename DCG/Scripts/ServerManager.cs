using Godot;
using System.Linq;

public partial class ServerManager : Node
{
	public static Client client;

	public override void _Ready()
	{
		client = new Client();

		client.Connect();
	}

	public override void _Process(double delta)
	{
		return;
	}

	// Defines a print function non-godot classes can use
	public static void Print(string str)
	{
		GD.Print(str);
	}

	// Disconnect the client on close
	public override void _ExitTree()
	{
		client.Disconnect();

		base._ExitTree();
	}
}
