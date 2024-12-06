using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Encounter : Time
    {
        public Encounter()
            : base(nameof(Turn), PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds)
        { }

        public Encounter(string ownerId)
            : base(nameof(Turn), PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds)
                { }
    }
}
