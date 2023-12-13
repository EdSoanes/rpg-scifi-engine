using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class Character : Artifact
    {
        [JsonConstructor] public Character() { }

        public Character(string name)
            : this()
        {
            Name = name;
        }

        [JsonProperty] public TurnPoints Turns { get; private set; } = new TurnPoints();
        [JsonProperty] public StatPoints Stats { get; private set; } = new StatPoints();
        [JsonProperty] public Container Equipment { get; private set; } = new Container();
        [JsonProperty] public Damage Damage { get; private set; } = new Damage();

        [Setup]
        public override Modifier[] Setup()
        {
            var mods = new List<Modifier>(base.Setup())
            {
                this.Mod((x) => x.Stats.StrengthBonus, (x) => x.Damage.Impact),
                this.Mod((x) => x.Stats.StrengthBonus, (x) => x.Turns.Exertion),
                this.Mod((x) => x.Stats.DexterityBonus, (x) => x.Turns.Action),
                this.Mod((x) => x.Stats.IntelligenceBonus, (x) => x.Turns.Focus)
            };

            return mods.ToArray();
        }
    }
}
