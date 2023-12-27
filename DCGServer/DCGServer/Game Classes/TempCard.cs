using MongoDB.Bson.Serialization;
using System;

namespace card
{
	public class TempCard
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Rank { get; set; }
		public double SacrificialValue { get; set; }
		public int Atk { get; set; }
		public int Hp { get; set; }
		public int Mana { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
		public string Img { get; set; }
		public string Pack { get; set; }
		public List<string> StatusName { get; set; }
		public List<int> StatusLength { get; set; }

		public TempCard()
		{
			StatusName = new();
			StatusLength = new();
		}

		public TempCard MakeReady()
		{
			TempCard temp = new()
			{
				Id = Id,
				Name = Name,
				Rank = Rank,
				SacrificialValue = SacrificialValue,
				Atk = Atk,
				Hp = Hp,
				Mana = Mana,
				Description = Description,
				Type = Type,
				Img = Img,
				Pack = Pack,
				StatusName = null,
				StatusLength = null
			};

			return temp;
		}
	}
}