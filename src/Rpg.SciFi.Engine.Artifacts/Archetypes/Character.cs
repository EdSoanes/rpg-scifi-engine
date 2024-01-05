using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Components.Containers;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public class Character : Actor
    {
        [JsonConstructor] public Character() { }

        public Character(string name)
            : this()
        {
            Name = name;
            Health = new Health(10, 10);

            AddContainer(new LeftHand());
            AddContainer(new RightHand());
            AddContainer(new Equipment());
        }

        [JsonProperty] public StatPoints Stats { get; private set; } = new StatPoints();
        [JsonProperty] public Damage Damage { get; private set; } = new Damage();

        [Setup]
        public override Modifier[] Setup()
        {
            var mods = new List<Modifier>(base.Setup())
            {
                BaseModifier.Create(this, "d6", x => x.Damage.Impact),
                BaseModifier.Create(this, x => x.Stats.StrengthBonus, x => x.Damage.Impact),
                BaseModifier.Create(this, x => x.Stats.StrengthBonus, x => x.Turns.Exertion),
                BaseModifier.Create(this, x => x.Stats.DexterityBonus, x => x.Turns.Action),
                BaseModifier.Create(this, x => x.Stats.IntelligenceBonus, x => x.Turns.Focus),
            };

            return mods.ToArray();
        }
    }
}
