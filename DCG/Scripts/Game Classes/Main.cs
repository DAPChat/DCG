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
        Card cardtest = new(list.ToArray(), 0);

        var thescene = ResourceLoader.Load<PackedScene>("res://Scenes/card.tscn").Instantiate();
        /*thescene = ResourceLoader.Load<CSharpScript>("res://Scripts/Card.cs").New();*/
        AddChild(thescene);
        

    }

    public override void _Ready()
    {
        Run();
    }
}