using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Actions;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public abstract class Actor : Artifact
    {
        [JsonConstructor] public Actor() { }

        [JsonProperty] public TurnPoints Turns { get; private set; } = new TurnPoints();

        [Ability]
        public virtual BaseAction Move(int distance)
        {
            var actionCost = Convert.ToInt32(Math.Ceiling((double)distance / this.Acceleration));

            return new TurnAction(Graph!, nameof(Move), actionCost, 1, 0)
                .OnSuccess(TurnModifier.Create(this, distance, x => x.CurrentSpeed))
                .OnSuccess(TurnModifier.Create(this, 10, x => x.Emissions.Sound.Value));
        }

        [Ability]
        public virtual BaseAction Evade(int cost)
        {
            return new TurnAction(Graph!, nameof(Evade), cost, cost, 1)
                .OnSuccess(TurnModifier.Create(this, cost, x => x.MeleeDefence))
                .OnSuccess(TurnModifier.Create(this, cost, x => x.MissileDefence));
        }

        [Ability]
        public virtual BaseAction Transfer(Container source, Container target, Artifact artifact)
        {
            return new TransferItemAction(Graph!, nameof(Transfer), source, target, artifact);
        }
    }
}
