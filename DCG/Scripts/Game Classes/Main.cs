using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    class CardObject
    {
        public string? Name { get; set; }
        public string? Rank { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public int ATK { get; set; }
        public int HP { get; set; }
        public string? Image { get; set; }
    }
    static void Main(string[] args)
    {
        //dynamic[] dict = [new CardObject { Name = "random", Rank = "S", Description = "akdsjfhlaksdfh", Type = "Spell", ATK = 10000, HP = 10000, Image = "https://publicfiles.dapchat.repl.co/dcgtest/reaper.jpg" }];
        //Card cardtest = new(dict, 0);
    }
}