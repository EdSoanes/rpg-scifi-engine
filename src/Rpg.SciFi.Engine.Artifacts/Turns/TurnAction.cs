using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class TurnAction : ModifierStore
    {
        [JsonConstructor] private TurnAction() { }

        public TurnAction(TurnPoints costs, State success)
        {
            Costs = costs;
            States = new States(success);
        }

        public TurnAction(TurnPoints costs, State success, State failure)
        {
            Costs = costs;
            States = new States(success, failure);
        }

        [JsonProperty] public TurnPoints Costs { get; private set; }
        [JsonProperty] public States States { get; private set; }

        [Moddable] public Dice DiceRoll { get => Evaluate(nameof(DiceRoll)); }
    }
}
