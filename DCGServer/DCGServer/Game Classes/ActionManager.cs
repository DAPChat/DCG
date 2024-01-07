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
            card = CreateCard(_action, _game);
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

    public static BaseCard CreateCard(CAP _action, game.Game _game)
    {
        return (BaseCard)Activator.CreateInstance(Type.GetType("card." + _action.card.Name.Replace(' ', '_')), new object[] { _action, _game });
    }

    public static void UpdateCards(TempCard[] cards, int id, game.Game game)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];

            if (card == null) continue;

            var b = CreateCard(new CAP { targetSlot = i, card = card, placerId = id }, game);

            b.Update();
        }
    }
}