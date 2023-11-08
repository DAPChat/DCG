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
        list.Append(new CardObject { Name = "random", Rank = "S", Description = "akdsjfhlaksdfh", Type = "Spell", ATK = 10000, HP = 10000, Image = "https://publicfiles.dapchat.repl.co/dcgtest/reaper.jpg" });
        Card cardtest = new(list.ToArray(), 0);

        var c = GD.Load<Node3D>("res://Scenes/card.tscn");
        AddChild(c);
    }

    public override void _Ready()
    {
        Run();
    }
}