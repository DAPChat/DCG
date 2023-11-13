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
        public string? Name { get; set; }
        public string? Rank { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public int ATK { get; set; }
        public int HP { get; set; }
        public string? Image { get; set; }
    }
    public void Run ()
    {
        List<CardObject> list = new List<CardObject>();
        list.Add(new CardObject { Name = "random", Rank = "S", Description = "akdsjfhlaksdfh", Type = "Spell", ATK = 10000, HP = 10000, Image = "https://publicfiles.dapchat.repl.co/dcgtest/reaper.jpg" });
        

        //var thescene = ResourceLoader.Load<CSharpScript>("res://Scripts/Card.cs").New();

        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate();
        
        AddChild(thescene);
        //Card cardtest = new(list.ToArray(), 0, thescene.NodePath);
        //thescene.SetScript("res://Scripts/Card.cs");
        //thescene.Call("setCard", [list.ToArray(), 0]);

        Card c = thescene as Card;

        c.setCard(list.ToArray(), 0);
        
    }

    public override void _Ready()
    {
        Run();
    }
}