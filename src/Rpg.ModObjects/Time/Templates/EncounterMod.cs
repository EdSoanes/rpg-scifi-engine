using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time.Templates
{
    public class EncounterMod : ModTemplate
    {
        public EncounterMod()
        {
            Lifecycle = new TimeLifecycle(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            Behavior = new Add(ModType.Standard);
        }
    }
}
