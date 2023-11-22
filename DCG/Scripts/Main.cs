using Godot;
using System;

public partial class Main : Node
{

	static LineEdit LUsername;
    static LineEdit LPassword;

	static LineEdit SUsername;
	static LineEdit SPassword;
	static LineEdit SCPassword;

	static Button LButton;
	static Button LSButton;
	
	static Button SButton;

	static Button PButton;

	static CanvasLayer LoginLayer;
	static CanvasLayer SignupLayer;
	static CanvasLayer HomeLayer;

	static Label LError;

	public static bool inGame = false;
	static bool lastSetting = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LUsername = (LineEdit)GetNode("Login/UI/Login/Username");
		LPassword = (LineEdit)GetNode("Login/UI/Login/Password");

		SUsername = (LineEdit)GetNode("Signup/UI/Signup/Username");
		SPassword = (LineEdit)GetNode("Signup/UI/Signup/Password");
		SCPassword = (LineEdit)GetNode("Signup/UI/Signup/ConfirmPassword");

		LButton = (Button)GetNode("Login/UI/Login/Login");
		LSButton = (Button)GetNode("Login/UI/Login/Signup");

		SButton = (Button)GetNode("Signup/UI/Signup/Signup");

		PButton = (Button)GetNode("Home/UI/Home/Play");

		LoginLayer = (CanvasLayer)GetNode("Login/UI");
		SignupLayer = (CanvasLayer)GetNode("Signup/UI");
		HomeLayer = (CanvasLayer)GetNode("Home/UI");

		LError = (Label)GetNode("Login/UI/Login/Error");

		LButton.Pressed += () => Login();
		LSButton.Pressed += () => LSignup();
		SButton.Pressed += () => Signup();
		PButton.Pressed += () =>
		{
			ServerManager.client.WriteStream(PacketManager.ToJson(new CSP()));

			PButton.SetDeferred(Button.PropertyName.Text, "Queued...");
			PButton.SetDeferred(BaseButton.PropertyName.Disabled, true);
		};

		if (ServerManager.client.account == null)
		{
			LoginLayer.Show();
			HomeLayer.Hide();
		}
		else
		{
			HomeLayer.Show();
			LoginLayer.Hide();
		}

		SignupLayer.Hide();
		LError.Hide();
	}

	public void Login()
	{
		string username = LUsername.Text.Trim();
		string password = LPassword.Text.Trim();

		if (username == "" || username == null || password == "" || password == null)
		{
			return;
		}

        LButton.SetDeferred(BaseButton.PropertyName.Disabled, true);

        ServerManager.client.WriteStream(PacketManager.ToJson(new ACP(false, username, password)));
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

        SButton.SetDeferred(BaseButton.PropertyName.Disabled, true);

        ServerManager.client.WriteStream(PacketManager.ToJson(new ACP(true, username, password)));
	}

	public static void Retry(string error)
	{
		SignupLayer.CallDeferred(CanvasLayer.MethodName.Hide);
		HomeLayer.CallDeferred(CanvasLayer.MethodName.Hide);
		LoginLayer.CallDeferred(CanvasLayer.MethodName.Show);

		LError.CallDeferred(CanvasItem.MethodName.Show);
		LError.CallDeferred(LineEdit.MethodName.SetText, error);

        SButton.SetDeferred(BaseButton.PropertyName.Disabled, false);
        SButton.SetDeferred(BaseButton.PropertyName.Disabled, false);
    }

    public static void Success()
	{
        SignupLayer.CallDeferred(CanvasLayer.MethodName.Hide);
        HomeLayer.CallDeferred(CanvasLayer.MethodName.Show);
        LoginLayer.CallDeferred(CanvasLayer.MethodName.Hide);

        LError.CallDeferred(CanvasItem.MethodName.Hide);

		LButton.SetDeferred(BaseButton.PropertyName.Disabled, false);
		SButton.SetDeferred(BaseButton.PropertyName.Disabled, false);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (lastSetting = inGame)
			if (inGame)
			{
				GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
				lastSetting = inGame;
			}
			else
			{
				lastSetting = inGame;
			}
	}
}
