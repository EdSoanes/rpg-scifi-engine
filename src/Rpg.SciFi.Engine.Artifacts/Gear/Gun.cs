using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Gear
{
    public class Gun : Artifact
    {
        public Gun()
        {
            Name = nameof(Gun);
            Damage = new Damage("d6", "d8", 0, 0, 0);
        }

        [JsonProperty] public int BaseRange { get; private set; }
        [JsonProperty] public int BaseAttack { get; private set; }
        [JsonProperty] public Damage Damage { get; private set; }

        [Moddable] public int Range { get => this.Resolve(nameof(Range)); }
        [Moddable] public int Attack { get => this.Resolve(nameof(Attack)); }

        [Ability()]
        [Input(Param = "character", BindsTo = "Character")]
        [Input(InputSource = InputSource.Player, Param = "target", BindsTo = "Target")]
        public TurnAction Fire(Character character, Artifact target)
        {
            character.Describe(nameof(Character.Stats.MissileAttackBonus))
            return new TurnAction
            {
                ActionPoints = 3,
                Exertion = 1,
                Focus = 1,
                Modifiers = new[]
                {
                    target.AddMod(ModType.Instant)
                }

            }
        }
    }
}
