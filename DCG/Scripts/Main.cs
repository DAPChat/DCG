using Godot;
using System;

public partial class Main : Node
{

	static TextEdit LUsername;
    static TextEdit LPassword;

	static TextEdit SUsername;
	static TextEdit SPassword;
	static TextEdit SCPassword;

	static Button LButton;
	static Button LSButton;
	
	static Button SButton;

	static CanvasLayer LoginLayer;
	static CanvasLayer SignupLayer;
	static CanvasLayer HomeLayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LUsername = (TextEdit)GetNode("Login/UI/Login/Username");
		LPassword = (TextEdit)GetNode("Login/UI/Login/Password");

		SUsername = (TextEdit)GetNode("Signup/UI/Signup/Username");
		SPassword = (TextEdit)GetNode("Signup/UI/Signup/Password");
		SCPassword = (TextEdit)GetNode("Signup/UI/Signup/ConfirmPassword");

		LButton = (Button)GetNode("Login/UI/Login/Login");
		LSButton = (Button)GetNode("Login/UI/Login/Signup");

		SButton = (Button)GetNode("Signup/UI/Signup/Signup");

		LoginLayer = (CanvasLayer)GetNode("Login/UI");
		SignupLayer = (CanvasLayer)GetNode("Signup/UI");
		HomeLayer = (CanvasLayer)GetNode("Home/UI");

		LButton.Pressed += () => Login();
		LSButton.Pressed += () => LSignup();
		SButton.Pressed += () => Signup();

		LoginLayer.Show();
		HomeLayer.Hide();
		SignupLayer.Hide();
	}

	public void Login()
	{
		string username = LUsername.Text.Trim();
		string password = LPassword.Text.Trim();

		if (username == "" || username == null || password == "" || password == null)
		{
			return;
		}

		ServerManager.client.WriteStream(PacketManager.ToJson(new ACP(false, username, password)));

		LoginLayer.Hide();
		HomeLayer.Show();
	}

	public void LSignup()
	{
        string username = LUsername.Text.Trim();
        string password = LPassword.Text.Trim();

		SPassword.Text = password;
		SUsername.Text = username;

		LoginLayer.Hide();
		SignupLayer.Show();
    }

	public void Signup()
	{
		string username = SUsername.Text.Trim();
		string password = SPassword.Text.Trim();
		string cPassword = SCPassword.Text.Trim();

		if (password != cPassword) return;

		ServerManager.client.WriteStream(PacketManager.ToJson(new ACP(true, username, password)));

        SignupLayer.Hide();
		HomeLayer.Show();
	}

	public static void Retry()
	{
		SignupLayer.CallDeferred(CanvasLayer.MethodName.Hide);
		HomeLayer.CallDeferred(CanvasLayer.MethodName.Hide);
		LoginLayer.CallDeferred(CanvasLayer.MethodName.Show);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
