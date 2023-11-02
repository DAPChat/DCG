using Godot;

public partial class ServerManager : Node
{
    Label idLabel;
    Button connectButton;
    TextureRect p;

    public override void _Ready()
    {
        idLabel = (Label)GetNode("/root/ServerManager/CanvasLayer/Label");
        connectButton = (Button)GetNode("/root/ServerManager/CanvasLayer/Button");

        // School IP: 10.72.100.135
        connectButton.ButtonDown += () => {
            connectButton.Hide();
            Client.Connect();
        };
    }

    public override void _Process(double delta)
    {
        // Update the current game id for display
        if (Client.str != null)
        {
            idLabel.Text = Client.str;
            Client.str = null;
        }
    }
}