using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public class TimeTypes
    {
        public const int PermanentDuration = int.MaxValue;
        public const int EncounterDuration = PermanentDuration - 1;
        public const int ExpiredDuration = int.MinValue;
        
        public const string BeforeEncounter = nameof(BeforeEncounter);
        public const string EncounterStart = nameof(EncounterStart);
        public const string Encounter = nameof(Encounter);
        public const string EncounterEnd = nameof(EncounterEnd);

        public static string[] All { get; private set; } = new []
        {
            BeforeEncounter,
            EncounterStart,
            Encounter,
            EncounterEnd
        };

        public static int AsTurn(string timeType)
        {
            return timeType switch
            {
                BeforeEncounter => -1,
                EncounterStart => 0,
                EncounterEnd => int.MaxValue - 1,
                _ => 1
            };
        }
    }
}
