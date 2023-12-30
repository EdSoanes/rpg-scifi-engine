using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Actor : Artifact
    {
        [JsonConstructor] public Actor() { }

        [JsonProperty] public TurnPoints Turns { get; private set; } = new TurnPoints();

        public Turns.Action Act(Entity target, string actionName, int actionCost, int exertionCost, int focusCost)
        {
            var action = new Turns.Action(actionName, actionCost, exertionCost, focusCost);
            Context.Add(action);

            return action;
        }
    }
}
