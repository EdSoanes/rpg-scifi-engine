using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Actions;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public abstract class Artifact : Entity
    {
        [JsonProperty] public EmissionSignature Emissions { get; protected set; } = new EmissionSignature();
        [JsonProperty] public Resistances Resistances { get; protected set; } = new Resistances();
        [JsonProperty] public Health Health { get; protected set; } = new Health();
        [JsonProperty] public States States { get; protected set; } = new States();

        [Moddable] public int Size { get => Resolve(); }

        [Moddable] public int Weight { get => Resolve(); }

        [Moddable] public int CurrentSpeed { get => Resolve(); }
        [Moddable] public int MaxSpeed { get => Resolve(); }
        [Moddable] public int Acceleration { get => Resolve(); }

        [Moddable] public int MeleeDefence { get => Resolve(); }
        [Moddable] public int MissileDefence { get => Resolve(); }

        public Guid? ContainerId { get; set; }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, 0, x => MaxSpeed),
                BaseModifier.Create(this, 0, x => CurrentSpeed),
                BaseModifier.Create(this, 0, x => Acceleration),
                BaseModifier.Create(this, 1, x => Size),
                BaseModifier.Create(this, 1, x => Weight),
                BaseModifier.Create(this, x => Size, x => MeleeDefence),
                BaseModifier.Create(this, x => Size, x => MissileDefence),
                BaseModifier.Create(this, x => CurrentSpeed, x => MissileDefence, () => Rules.Minus)
            };
        }

        [Ability]
        public TurnAction Destroy()
        {
            return new TurnAction(Graph!, nameof(Destroy), 0, 0, 0);
        }
    }
}
