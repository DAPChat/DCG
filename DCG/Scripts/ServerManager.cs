using Godot;
using System.Linq;

public partial class ServerManager : Node
{
	Label idLabel;
	Button connectButton;
	TextureRect p;

	Client client;

	public override void _Ready()
	{
		client = new Client();

		client.Connect();

		return;

		idLabel = (Label)GetNode("/root/ServerManager/CanvasLayer/Label");
		connectButton = (Button)GetNode("/root/ServerManager/CanvasLayer/Button");

		// School IP: 10.72.100.135
		connectButton.ButtonDown += () =>
		{
			connectButton.Hide();
			client.Connect();
		};
	}

	public override void _Process(double delta)
	{
		return;

		string text = "";
		// Update the current game id for display
		for (int i = 0; i < client.values.Count; i++) 
		{
			if (client.values.Keys.ToList()[i] == "Game Id")
			{
				text += "Game Id: " + client.values.Values.ToList()[i] + "\n";
			}
			else
			{
				text += client.values.Keys.ToList()[i] + " : " + client.values.Values.ToList()[i] + "\n";

				if (client.values.Keys.ToList()[i] == "Connection")
					client.values["Connection"] = "0";
			}
		}

		idLabel.Text = text;
	}

	public override void _ExitTree()
	{
		client.client.Close();
		client.client.Dispose();
		base._ExitTree();
	}
}