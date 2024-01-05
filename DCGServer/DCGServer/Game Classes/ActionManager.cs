using card;
using packets;

public class ActionManager
{
    public static void Register(CAP _action, game.Game _game)
    {
        if (_action.card.Name == null) return;

        BaseCard card = null;

        player.Player p = _game.currentBoard.GetPlayer(_action.placerId);

        var field = _action.card.Class == "Spell" ? p.fieldRowTwo : p.fieldRowOne;

        if (field[_action.senderSlot].StatusName.Contains(_action.action)) return;

        try
        {
            card = (BaseCard)Activator.CreateInstance(Type.GetType("card." + _action.card.Name.Replace(' ', '_')), new object[] { _action, _game });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("{0} does not have a class.", _action.card.Name);

            return;
        }

        try
        {
            card.GetType().GetMethod(_action.action).Invoke(card, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("{0} does not contain the method, {1}", card.GetType().Name, _action.action);

            return;
        }

        _game.currentBoard.NextPhase();
    }
}