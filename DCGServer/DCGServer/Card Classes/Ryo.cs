using packets;
using System.Security.Cryptography;

namespace card
{
    public class Ryo : BaseCard
    {
        int i = new Random().Next(1,10);

        public Ryo (CAP action)
        {
            card = action.card;
            Console.WriteLine(action.card.Name);
        }

        public override void Run()
        {
            Console.WriteLine(i);
        }

        public void Attack()
        {
            Console.WriteLine("Hello");
        }
    }
}