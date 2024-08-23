using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Encounter : Time
    {
        [JsonConstructor] protected Encounter()
            : base()
        { }

        public Encounter(ModType modType = ModType.Standard)
            : base(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds, modType)
        { }
    }
}
