using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public interface IContext
    {
        Entity? Root { get; }
        MetaEntityStore Entities { get; }
        MetaModifierStore Mods { get; }
        TurnAction CreateTurnAction(string name, int actionCost, int exertionCost, int focusCost);
    }

    public class Meta<T> : IContext
        where T : Entity
    {
        public Entity? Root { get => Context; }
        public T? Context { get; private set; }
        public MetaEntityStore Entities { get; set; }
        public MetaModifierStore Mods { get; set; }

        public Meta()
        {
            Entities = new MetaEntityStore();
            Mods = new MetaModifierStore();

            Entities?.PropertyValue("Context", this as IContext);
            Mods?.PropertyValue("Context", this as IContext);
        }

        public void Initialize(T context)
        {
            Mods.Clear();
            Entities.Clear();
            Context = context;

            Entities.Add(Context);
            Entities.Setup();
        }

        public TurnAction CreateTurnAction(string name, int actionCost, int exertionCost, int focusCost)
        {
            var action = new TurnAction(name, actionCost, exertionCost, focusCost);
            Entities.Add(action);

            return action;
        }

        public TurnAction? Apply(Character actor, TurnAction turnAction, int diceRoll = 0)
        {
            var modifiers = turnAction.Resolve(diceRoll);

            var actionCost = turnAction.Action;
            if (actionCost != 0)
                Mods.Add(actor.Mod(nameof(TurnPoints.Action), actionCost, (x) => x.Turns.Action, () => Rules.Minus).IsAdditive());

            var exertionCost = turnAction.Exertion;
            if (exertionCost != 0)
                Mods.Add(actor.Mod(nameof(TurnPoints.Exertion), exertionCost, (x) => x.Turns.Exertion, () => Rules.Minus).IsAdditive());

            var focusCost = turnAction.Focus;
            if (focusCost != 0)
                Mods.Add(actor.Mod(nameof(TurnPoints.Focus), focusCost, (x) => x.Turns.Focus, () => Rules.Minus).IsAdditive());

            return turnAction.NextAction();
        }

        public string[] Describe() => Entities.Values.OrderBy(x => x.MetaData.Path).Select(x => x.ToString()).ToArray();

        public void Clear()
        {
            Context = null;
            Entities = null;
        }

        public string Serialize()
        {
            var json = JsonConvert.SerializeObject(Context, Formatting.None);
            return json;
        }

        public void Deserialize(string state)
        {
            var context = JsonConvert.DeserializeObject<T>(state);
            Initialize(context!);
        }
    }
}
