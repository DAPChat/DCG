using card;
using Newtonsoft.Json;
using packets;
using System;

public class ActionManager
{
    static Dictionary<string, List<int>> func = new();

    public static void AddAction(CAP _action)
    {
        string _actions = _action.card.Actions;

        if (_actions == null) return;

        Dictionary<string, List<int>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(_actions);

        func.Add(_action.action, dict[_action.action]);

        RegisterAction(_action.action);
    }
    static void RegisterAction(string action)
    {
        object obj = new();

        switch (action)
        {
            case "Attack":
                obj = new Attack(func["Attack"]);
                break;
        }

        if (obj != null && obj is BaseAction)
            ((BaseAction)obj).Run(null, null);
    }
}