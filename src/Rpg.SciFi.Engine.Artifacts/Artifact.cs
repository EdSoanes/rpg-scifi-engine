using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Artifact : Entity
    {
        [JsonProperty] public EmissionSignature Emissions { get; protected set; } = new EmissionSignature();
        [JsonProperty] public Resistances Resistances { get; protected set; } = new Resistances();
        [JsonProperty] public Health Health { get; protected set; } = new Health();
        [JsonProperty] public States States { get; protected set; } = new States();

        [Moddable] public int BaseSize { get => Resolve(nameof(BaseSize)); }
        [Moddable] public int BaseWeight { get => Resolve(nameof(BaseWeight)); }
        [Moddable] public int BaseSpeed { get => Resolve(nameof(BaseSpeed)); }
        [Moddable] public int BaseMeleeDefence { get => Resolve(nameof(BaseMeleeDefence)); }
        [Moddable] public int BaseMissileDefence { get => Resolve(nameof(BaseMissileDefence)); }

        [Moddable] public int Size { get => Resolve(nameof(Size)); }
        [Moddable] public int Weight { get => Resolve(nameof(Weight)); }
        [Moddable] public int Speed { get => Resolve(nameof(Speed)); }

        [Moddable] public int MeleeDefence { get => Resolve(nameof(MeleeDefence)); }
        [Moddable] public int MissileDefence { get => Resolve(nameof(MissileDefence)); }

        [Moddable] public bool Destroyed { get => Resolve(nameof(Destroyed)) > 0; }

        public virtual Modifier[] Setup()
        {
            return new[]
            {
                this.Mod(nameof(BaseSize), 1, (x) => BaseSize),
                this.Mod(nameof(BaseWeight), 1, (x) => BaseWeight),
                this.Mod(nameof(BaseSpeed), 0, (x) => BaseSpeed),
                this.Mod(nameof(BaseMeleeDefence), 0, (x) => BaseMeleeDefence),
                this.Mod(nameof(BaseMissileDefence), 0, (x) => BaseMissileDefence),

                this.Mod((x) => BaseSize, (x) => Size),
                this.Mod((x) => BaseWeight, (x) => Weight),
                this.Mod((x) => BaseSpeed, (x) => Speed),
                this.Mod((x) => BaseMeleeDefence, (x) => MeleeDefence),
                this.Mod((x) => BaseMissileDefence, (x) => MissileDefence),

                this.Mod((x) => Size, (x) => MeleeDefence),
                this.Mod((x) => Size, (x) => MissileDefence),
                this.Mod((x) => Speed, (x) => MissileDefence, () => Rules.Minus)
            };
        }

        [Ability]
        public TurnAction Destroy()
        {
            return new TurnAction(nameof(Destroy), 0, 0, 0);
        }
    }
}
