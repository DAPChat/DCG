using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

public partial class Main : Node3D 
{
    public class CardObject
    {
		public int Id { get; set; }
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
    public void Run ()
    {
        List<CardObject> list = new List<CardObject>();
        list.Add(new CardObject { Name = "random", Rank = "S", Description = "akdsjfhlaksdfh", Type = "Spell", Atk = 10000, Hp = 10000, Img = "https://publicfiles.dapchat.repl.co/dcgtest/reaper.jpg" });
        

        //var thescene = ResourceLoader.Load<CSharpScript>("res://Scripts/Card.cs").New();

        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate();
        
        AddChild(thescene);
        //Card cardtest = new(list.ToArray(), 0, thescene.NodePath);
        //thescene.SetScript("res://Scripts/Card.cs");
        //thescene.Call("setCard", [list.ToArray(), 0]);

        Card c = thescene as Card;

        c.setCard(list.ToArray(), 0);
        
    }

	public void CreateCard(CardObject _c)
	{
		List<CardObject> list = new List<CardObject>
		{
			_c
		};


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
        Run();
    }
}