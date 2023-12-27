public abstract class Action
{
    public abstract string name { get; }

    public abstract void Run(Card card);
    public abstract void Run(Card card, int slot);
}