using card;
using Newtonsoft.Json;
using packets;
using System;

public class ActionManager
{
    public static void GetClass(CAP _action)
    {
        if (_action.card.Name == null) return;

        Type type = Type.GetType("card." + _action.card.Name);

        object obj = Activator.CreateInstance(type);

        try
        {
            ((BaseCard)obj).GetType().GetMethod(_action.action).Invoke(obj, null);
        }catch (Exception)
        {
            Console.WriteLine("The type, {0}, does not contain the method, {1}", type.Name, _action.action);
        }
    }
}