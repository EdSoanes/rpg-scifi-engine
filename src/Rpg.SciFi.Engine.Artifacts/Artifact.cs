using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Artifact : Entity
    {
        public Artifact() 
        {
            Name = nameof(Artifact);
            BaseSize = 1;
            BaseWeight = 1;
            Emissions = new EmissionSignature();
            Resistances = new Resistances();
            Health = new Health();
            States = new States();
        }

        [JsonProperty] public string Name { get; protected set; }

        [JsonProperty] public EmissionSignature Emissions { get; protected set; }
        [JsonProperty] public Resistances Resistances { get; protected set; }
        [JsonProperty] public Health Health { get; protected set; }
        [JsonProperty] public States States { get; protected set; }

        [JsonProperty] public int BaseSize { get; protected set; }
        [JsonProperty] public int BaseWeight { get; protected set; }
        [JsonProperty] public int BaseSpeed { get; protected set; }
        [JsonProperty] public int BaseMeleeToHit { get; protected set; }
        [JsonProperty] public int BaseMissileToHit { get; protected set; }

        [Moddable] public int Size { get => Resolve(nameof(Size)); }
        [Moddable] public int Weight { get => Resolve(nameof(Weight)); }
        [Moddable] public int Speed { get => Resolve(nameof(Speed)); }

        [Moddable] public int MeleeToHit { get => Resolve(nameof(MeleeToHit)); }
        [Moddable] public int MissileToHit { get => Resolve(nameof(MissileToHit)); }

        [Moddable] public bool Destroyed { get => Resolve(nameof(Destroyed)) > 0; }

        public virtual void Setup()
        {
            this.Mod((x) => BaseSize, (x) => Size).IsBase().Apply();
            this.Mod((x) => BaseWeight, (x) => Weight).IsBase().Apply();
            this.Mod((x) => BaseSpeed, (x) => Speed).IsBase().Apply();

            this.Mod((x) => BaseMeleeToHit, (x) => MeleeToHit).IsBase().Apply();
            this.Mod((x) => Size, (x) => MeleeToHit).IsBase().Apply();

            this.Mod((x) => BaseMissileToHit, (x) => MissileToHit).IsBase().Apply();
            this.Mod((x) => Size, (x) => MissileToHit).IsBase().Apply();
            this.Mod((x) => Speed, (x) => MissileToHit, () => Rules.Minus).IsBase().Apply();
        }

        [Ability]
        public TurnAction Destroy()
        {
            return new TurnAction(0, 0, 0);
        }
    }
}
