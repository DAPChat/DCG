using packets;
using card;

public class ActionManager
{
    public static void GetClass(CAP _action)
    {
        if (_action.card.Name == null) return;

        BaseCard card = null;

        try
        {
            card = (BaseCard)Activator.CreateInstance(Type.GetType("card." + _action.card.Name), new object[] { _action });
        }
        catch (Exception)
        {
            Console.WriteLine("{0} does not have a class.", _action.card.Name);
        }

        if (card == null) return;

        try
        {
            card.GetType().GetMethod(_action.action).Invoke(card, null);
        }
        catch (Exception)
        {
            Console.WriteLine("{0} does not contain the method, {1}", card.GetType().Name, _action.action);
        }
    }
}