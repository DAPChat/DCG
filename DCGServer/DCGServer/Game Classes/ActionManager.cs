using card;
using packets;

public class ActionManager
{
    public static void GetClass(CAP _action, game.Game _game)
    {
        if (_action.card.Name == null) return;

        BaseCard card = null;

        try
        {
            card = (BaseCard)Activator.CreateInstance(Type.GetType("card." + _action.card.Name), new object[] { _action, _game });
        }
        catch (Exception e)
        {
            //Console.WriteLine(e.ToString());
            Console.WriteLine("{0} does not have a class.", _action.card.Name);

            return;
        }

        try
        {
            card.GetType().GetMethod(_action.action).Invoke(card, null);
        }
        catch (Exception e)
        {
            //Console.WriteLine(e.ToString());
            Console.WriteLine("{0} does not contain the method, {1}", card.GetType().Name, _action.action);

            return;
        }

        _game.currentBoard.NextPhase();
    }
}