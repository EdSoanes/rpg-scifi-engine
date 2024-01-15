using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rpg.Sys.Archetypes
{
    public class Fatigued : Condition
    {
        [JsonConstructor] private Fatigued() { }

        public Fatigued(Actor actor)
            : base(nameof(Fatigued))
        {
            Add(
                StateModifier.Create(nameof(Fatigued), actor, 2, x => x.Stats.Strength.Bonus, () => DiceCalculations.Minus),
                StateModifier.Create(nameof(Fatigued), actor, 2, x => x.Movement.Speed.Max, () => DiceCalculations.Minus)            );
        }
    }
}
