using Rpg.Experimental;
using Rpg.Experimental.Json;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.System;

namespace Rpg.Cyborgs.Tests.Models
{
    internal static class TestSystem
    {
        /// <summary>
        /// The Cyborgs system plus the test only objects (e.g. Room) defined in this assembly
        /// </summary>
        public static RpgSystem Build()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(CyborgsSystem).Assembly);
            return RpgSystemFactory.Build(new CyborgsSystem(), typeof(Room).Assembly);
        }

        /// <summary>
        /// Serialize and restore the character sheet as the sessionless server does on every request
        /// </summary>
        public static RpgCharacterSheet RoundTrip(RpgCharacterSheet characterSheet)
        {
            var json = RpgJson.SerializeGraphState(characterSheet.GetState());
            var state = RpgJson.DeserializeGraphState<RpgCharacterSheetState>(json);

            return new RpgCharacterSheet(state, characterSheet.GetSystem());
        }
    }
}
