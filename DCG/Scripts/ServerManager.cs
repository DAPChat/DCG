using Godot;
using System.Linq;

public partial class ServerManager : Node
{
	Client client;

	public override void _Ready()
	{
		client = new Client();

		client.Connect();
	}

	public override void _Process(double delta)
	{
		return;
	}

	public static void Print(string str)
	{
		GD.Print(str);
	}

	public override void _ExitTree()
	{
		client.client.Close();
		client.client.Dispose();
		base._ExitTree();
	}
}
