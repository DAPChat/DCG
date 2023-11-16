using Godot;
using System.Collections.Generic;

public partial class Main : Node3D 
{
	public class CardObject
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Rank { get; set; }
		public int SacrificialValue { get; set; }
		public int Atk { get; set; }
		public int Hp { get; set; }
		public int Mana { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
		public string Img { get; set; }
		public string Pack { get; set; }
		public string CurrentStatus { get; set; }
	}

	private bool CameraView = true;

	MeshInstance3D mesh;
	Vector3 meshPos;

	public static CardObject cardObject = null;

	public void Run ()
	{
		List<CardObject> list = new List<CardObject>();
		list.Add(new CardObject { Name = "random", Rank = "S", Description = "akdsjfhlaksdfh", Type = "Spell", Atk = 10000, Hp = 10000, Img = "dcgtest/reaper.jpg" });
		

		//var thescene = ResourceLoader.Load<CSharpScript>("res://Scripts/Card.cs").New();

		var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate();
		var cardPosition = GetNode<MeshInstance3D>("Player1/Spell1").Position;
		AddChild(thescene);
		//Card cardtest = new(list.ToArray(), 0, thescene.NodePath);
		//thescene.SetScript("res://Scripts/Card.cs");
		//thescene.Call("setCard", [list.ToArray(), 0]);

		Card c = thescene as Card;

		c.setCard(list.ToArray(), 0, cardPosition);
		
	}
	public override void _Ready()
	{
        Button button = (Button)GetNode("/root/Game/Camera3D/CanvasLayer/Control/ChangeView");
        Camera3D cam1 = (Camera3D)GetNode("/root/Game/Camera3D");
        Camera3D cam2 = (Camera3D)GetNode("/root/Game/Camera3D2");

        cam1.MakeCurrent();

        button.ButtonDown += () =>
        {
            if (CameraView)
            {
                cam2.MakeCurrent();
                CameraView = false;
            }
            else
            {
                cam1.MakeCurrent();
                CameraView = true;
            }
        };

        mesh = GetNode<MeshInstance3D>("Player1/Spell1");
		meshPos = mesh.Position;

    }

    public override void _Process(double delta)
    {
		if (cardObject != null)
		{
            List<CardObject> list = new List<CardObject>{ cardObject };

            //var thescene = ResourceLoader.Load<CSharpScript>("res://Scripts/Card.cs").New();

            var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate();
            var cardPosition = meshPos;
            AddChild(thescene);
            //Card cardtest = new(list.ToArray(), 0, thescene.NodePath);
            //thescene.SetScript("res://Scripts/Card.cs");
            //thescene.Call("setCard", [list.ToArray(), 0]);

            Card c = thescene as Card;

            c.setCard(list.ToArray(), 0, cardPosition);

			cardObject = null;
        }

        base._Process(delta);
    }
}
