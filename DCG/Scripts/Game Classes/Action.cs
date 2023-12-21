public abstract class Action
{
    public abstract string name { get; }

    public abstract void Run(GameScene.CardObject card);
    public abstract void Run(GameScene.CardObject card, int slot);
}