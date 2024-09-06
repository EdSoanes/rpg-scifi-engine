using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Encounter : Time
    {
        public Encounter()
            : base(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds)
        { }
    }
}
