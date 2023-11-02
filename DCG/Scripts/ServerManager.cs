using Godot;

public partial class ServerManager : Node
{
    Label label;

    public override void _Ready()
    {
        label = (Label)GetNode("/root/ServerManager/CanvasLayer/Label");

        // School IP: 10.72.100.135
        Client.Connect();
    }

    public override void _Process(double delta)
    {
        if (Client.str != null)
        {
            label.Text = Client.str;
            Client.str = null;
        }
    }
}