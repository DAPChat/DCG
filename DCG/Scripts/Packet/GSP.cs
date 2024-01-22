namespace packets
{
	public class GSP : Packet
	{
		public int gameId;
		public int turn;
		public int phase;

        public string winner;
        public bool end = false;
    }
}