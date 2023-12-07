using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Artifact : Entity
    {
        public Artifact() 
        {
            BaseSize = 1;
            BaseWeight = 1;
            Emissions = new EmissionSignature();
            Resistances = new Resistances();
            Health = new Health();
            States = new States();
        }

        [JsonProperty] public EmissionSignature Emissions { get; protected set; }
        [JsonProperty] public Resistances Resistances { get; protected set; }
        [JsonProperty] public Health Health { get; protected set; }
        [JsonProperty] public States States { get; protected set; }

        [JsonProperty] public int BaseSize { get; protected set; }
        [JsonProperty] public int BaseWeight { get; protected set; }
        [JsonProperty] public int BaseSpeed { get; protected set; }
        [JsonProperty] public int BaseMeleeDefence { get; protected set; }
        [JsonProperty] public int BaseMissileDefence { get; protected set; }

        [Moddable] public int Size { get => Resolve(nameof(Size)); }
        [Moddable] public int Weight { get => Resolve(nameof(Weight)); }
        [Moddable] public int Speed { get => Resolve(nameof(Speed)); }

        [Moddable] public int MeleeDefence { get => Resolve(nameof(MeleeDefence)); }
        [Moddable] public int MissileDefence { get => Resolve(nameof(MissileDefence)); }

        [Moddable] public bool Destroyed { get => Resolve(nameof(Destroyed)) > 0; }

        public virtual void Setup()
        {
            this.Mod((x) => BaseSize, (x) => Size).IsBase().Apply();
            this.Mod((x) => BaseWeight, (x) => Weight).IsBase().Apply();
            this.Mod((x) => BaseSpeed, (x) => Speed).IsBase().Apply();

            this.Mod((x) => BaseMeleeDefence, (x) => MeleeDefence).IsBase().Apply();
            this.Mod((x) => Size, (x) => MeleeDefence).IsBase().Apply();

            this.Mod((x) => BaseMissileDefence, (x) => MissileDefence).IsBase().Apply();
            this.Mod((x) => Size, (x) => MissileDefence).IsBase().Apply();
            this.Mod((x) => Speed, (x) => MissileDefence, () => Rules.Minus).IsBase().Apply();
        }

        [Ability]
        public TurnAction Destroy()
        {
            return new TurnAction(0, 0, 0);
        }
    }
}
