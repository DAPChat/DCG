﻿using MongoDB.Bson;
using System;

public class Card
{
	public ObjectId Id { get; set; }
	public string Name { get; set; }
	public string Rank { get; set; }
	public int SacrificialValue { get; set; }
	public int Atk {  get; set; }
	public int Hp { get; set; }
	public int Mana { get; set; }
	public string Description {  get; set; }
	public string Type { get; set; }
	public string Img { get; set; }
	public string Pack {  get; set; }
	public string CurrentStatus { get; set; }
}