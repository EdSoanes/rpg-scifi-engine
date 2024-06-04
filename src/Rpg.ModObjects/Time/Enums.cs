using Rpg.ModObjects.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public static class Lifecycle
    {
        public static TimeLifecycle Turn()
            => Turn(0, 1);

        public static TimeLifecycle Turn(int duration)
            => Turn(0, duration);

        public static TimeLifecycle Turn(int delay, int duration)
            => new TimeLifecycle(
                new Time(TimeTypes.Encounter, delay),
                new Time(TimeTypes.Encounter, duration));

        public static TimeLifecycle Encounter()
            => new TimeLifecycle(
                TurnBasedTimeEngine.EncounterStart,
                TurnBasedTimeEngine.EncounterEnd);
    }

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
