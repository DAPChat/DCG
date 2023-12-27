using Godot;

using packets;
using deck;
public partial class Main : Node
{

	static LineEdit LUsername;
	static LineEdit LPassword;
	static LineEdit SUsername;
	static LineEdit SPassword;
	static LineEdit SCPassword;

	static Button LButton;
	static Button LSButton;
    static Button LeftButton;
    static Button RightButton;
    static Button SButton;
	static Button PButton;

	static CanvasLayer LoginLayer;
	static CanvasLayer SignupLayer;
	static CanvasLayer HomeLayer;

    static Control HomeControl;
    static Control EditorControl;

	static Label LError;
	static Main instance;

    static bool lastSetting = false;

    public static bool inGame = false;
    public static bool reload = false;

	public int TabBar = 1;

	public override void _Ready()
	{
		instance = this;

		LUsername = (LineEdit)GetNode("Login/UI/Login/Username");
		LPassword = (LineEdit)GetNode("Login/UI/Login/Password");

		SUsername = (LineEdit)GetNode("Signup/UI/Signup/Username");
		SPassword = (LineEdit)GetNode("Signup/UI/Signup/Password");
		SCPassword = (LineEdit)GetNode("Signup/UI/Signup/ConfirmPassword");

		LButton = (Button)GetNode("Login/UI/Login/Login");
		LSButton = (Button)GetNode("Login/UI/Login/Signup");
        LeftButton = (Button)GetNode("Home/UI/Defualt/LeftArrow");
        RightButton = (Button)GetNode("Home/UI/Defualt/RightArrow");

        SButton = (Button)GetNode("Signup/UI/Signup/Signup");

		PButton = (Button)GetNode("Home/UI/Home/Play");

		LoginLayer = (CanvasLayer)GetNode("Login/UI");
		SignupLayer = (CanvasLayer)GetNode("Signup/UI");
		HomeLayer = (CanvasLayer)GetNode("Home/UI");
        HomeControl = (Control)GetNode("Home/UI/Home");
        EditorControl = (Control)GetNode("Home/UI/Editor");

        LError = (Label)GetNode("Login/UI/Login/Error");
        LeftButton.Pressed += () => TabChange(-1);
        RightButton.Pressed += () => TabChange(1);
        LButton.Pressed += () => Login();
		LSButton.Pressed += () => LSignup();
		SButton.Pressed += () => Signup();
		PButton.Pressed += () =>
		{
			// Asks the server to queue the player
			ServerManager.client.WriteStream(PacketManager.ToJson(new CSP()));

			PButton.SetDeferred(Button.PropertyName.Text, "Queued...");
			PButton.SetDeferred(BaseButton.PropertyName.Disabled, true);
		};

		// Sets the player to the correct screen
		if (ServerManager.client.account == null)
		{
			LoginLayer.Show();
			LUsername.GrabFocus();
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

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("login"))
		{
			if (ServerManager.client.account != null)
			{
				if (PButton.Disabled == true) return;
				ServerManager.client.WriteStream(PacketManager.ToJson(new CSP()));

				PButton.SetDeferred(Button.PropertyName.Text, "Queued...");
				PButton.SetDeferred(BaseButton.PropertyName.Disabled, true);
			}
			else if (!LButton.Disabled == true)
			{
				Login();
			}
		}
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

		// Sends the packet to the server for verification
		ServerManager.client.WriteStream(PacketManager.ToJson(new ACP(false, username, password)));
	}

	public void LSignup()
	{
		// Changes the screen to the login screen and sets text
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

		// Tells the server to create an account
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
		LButton.SetDeferred(BaseButton.PropertyName.Disabled, false);
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
	public void TabChange(int Change)
	{
		
		int TabChanger = TabBar + Change;

        switch(TabChanger) {
			case -1:
				TabBar = 1;
				HomeControl.Show();
				EditorControl.Hide();
				break;
			case 0:
				HomeControl.Hide();
				Editor.LoadCardsIn();
				EditorControl.Show();
				TabBar += Change;
				break;
			case 1:
				HomeControl.Show();
				EditorControl.Hide();
				TabBar += Change;
				break;
			case 2:
				TabBar = 0;
				HomeControl.Hide();
				EditorControl.Show();
				break;
		}
	}
	
	public override void _Process(double delta)
	{
		if (lastSetting != inGame)
			if (inGame)
			{
                lastSetting = inGame;
				GameScene.changeScene = false;
                GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
			}
			else
			{
				lastSetting = inGame;
			}

		if (reload)
		{
			reload = false;

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
	}
}
