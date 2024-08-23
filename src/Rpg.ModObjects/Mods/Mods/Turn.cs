using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Turn : Time
    {
        [JsonConstructor] protected Turn()
            : base()
        { }

        public Turn(ModType modType = ModType.Standard)
            : base(0, 1)
        { }

        public Turn(int startTurn, ModType modType = ModType.Standard)
            : base(startTurn, 1, modType)
        { }

        public Turn(int startTurn, int duration, ModType modType = ModType.Standard)
            : base(startTurn, duration, modType)
        { }
    }
}
