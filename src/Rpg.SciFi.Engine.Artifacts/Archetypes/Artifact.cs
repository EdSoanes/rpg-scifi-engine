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

        [Moddable] public int BaseSize { get => Resolve(); }
        [Moddable] public int BaseWeight { get => Resolve(); }
        [Moddable] public int BaseSpeed { get => Resolve(); }
        [Moddable] public int BaseMeleeDefence { get => Resolve(); }
        [Moddable] public int BaseMissileDefence { get => Resolve(); }

        [Moddable] public int Size { get => Resolve(); }
        [Moddable] public int Weight { get => Resolve(); }
        [Moddable] public int Speed { get => Resolve(); }

        [Moddable] public int MeleeDefence { get => Resolve(); }
        [Moddable] public int MissileDefence { get => Resolve(); }

        [Moddable] public bool Destroyed { get => Resolve() > 0; }

        public Guid? ContainerId { get; set; }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, 1, x => BaseSize),
                BaseModifier.Create(this, 1, x => BaseWeight),
                BaseModifier.Create(this, 0, x => BaseSpeed),
                BaseModifier.Create(this, 0, x => BaseMeleeDefence),
                BaseModifier.Create(this, 0, x => BaseMissileDefence),

                BaseModifier.Create(this, x => BaseSize, x => Size),
                BaseModifier.Create(this, x => BaseWeight, x => Weight),
                BaseModifier.Create(this, x => BaseSpeed, x => Speed),
                BaseModifier.Create(this, x => BaseMeleeDefence, x => MeleeDefence),
                BaseModifier.Create(this, x => BaseMissileDefence, x => MissileDefence),

                BaseModifier.Create(this, x => Size, x => MeleeDefence),
                BaseModifier.Create(this, x => Size, x => MissileDefence),
                BaseModifier.Create(this, x => Speed, x => MissileDefence, () => Rules.Minus)
            };
        }

        [Ability]
        public TurnAction Destroy()
        {
            return new TurnAction(ModStore!, Evaluator!, nameof(Destroy), 0, 0, 0);
        }
    }
}
