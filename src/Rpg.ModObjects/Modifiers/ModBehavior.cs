using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Modifiers
{
    public abstract class ModBehavior
    {
        public const int PermanentMin = int.MinValue;
        public const int PermanentMax = int.MaxValue;
        public const int BeforeEncounter = -1;
        public const int EncounterStart = 0;
        public const int EncounterEnd = int.MaxValue - 1;

        [JsonProperty] public ModType Type { get; protected set; } = ModType.Standard;
        [JsonProperty] public ModMerging Merging { get; protected set; } = ModMerging.Add;
        [JsonProperty] public int Delay { get; protected set; } = int.MinValue;
        [JsonProperty] public int Duration { get; protected set; } = int.MaxValue;
        [JsonProperty] public int TurnAdded { get; protected set; }
        [JsonProperty] public ModExpiry Expiry { get; protected set; } = ModExpiry.Active;

        public int StartTurn
        {
            get
            {
                if (Delay < 0 || TurnAdded < EncounterStart)
                    return PermanentMin;

                return TurnAdded + Delay;
            }
        }
        
        public int EndTurn
        {
            get
            {
                if (Duration < EncounterStart)
                    return PermanentMin;

                if (Duration >= EncounterEnd)
                    return Duration;

                return StartTurn + Duration - 1;
            }
        }

        public void OnAdd(int turn)
            => TurnAdded = turn;

        public void SyncWith(ModBehavior behavior)
        {
            Delay = behavior.Delay;
            Duration = behavior.Duration;
            TurnAdded = behavior.TurnAdded;
        }

        public void SetExpired()
        {
            Delay = PermanentMin;
            Duration = PermanentMin;
        }

        public virtual void SetExpiry(RpgGraph graph, Mod? mod = null)
        {
            if (EndTurn < graph.Turn || (EndTurn == EncounterEnd && graph.Turn == EncounterEnd))
            {
                if (graph.Turn == EncounterEnd || graph.Turn == EncounterStart || graph.Turn == BeforeEncounter)
                    Expiry = ModExpiry.Remove;
                else
                    Expiry = ModExpiry.Expired;

                return;
            }

            if (StartTurn > graph.Turn)
            {
                Expiry = ModExpiry.Pending;
                return;
            }

            Expiry = ModExpiry.Active;
        }
    }

    public class Initial : ModBehavior
    {
        public Initial() 
        {
            Merging = ModMerging.Replace;
            Type = ModType.Initial;
        }
    }

    public class Base : ModBehavior
    {
        public Base()
        {
            Merging = ModMerging.Replace;
            Type = ModType.Base;
        }
    }

    public class Override : ModBehavior
    {
        public Override()
        {
            Merging = ModMerging.Replace;
            Type = ModType.Override;
        }
    }

    public class State : ModBehavior
    {
        public State(int delay, int duration)
        {
            Merging = ModMerging.Combine;
            Type = ModType.State;
            Delay = delay;
            Duration = duration;
        }

        public State()
        {
            Merging = ModMerging.Combine;
            Type = ModType.State;
        }

        public State(int duration)
            : this(0, duration) { }

        public override void SetExpiry(RpgGraph graph, Mod? mod = null)
        {
            var res = graph.CalculateModValue(mod);
            Expiry = res == Dice.Zero
                ? ModExpiry.Expired
                : ModExpiry.Active;
        }
    }

    public class ForceState : ModBehavior
    {
        public ForceState()
        {
            Merging = ModMerging.Replace;
            Type = ModType.ForceState;
        }
    }

    public class Permanent : ModBehavior
    { }

    public class Synced : ModBehavior
    {
        public Synced()
        {
            Type = ModType.Synced;
        }
    }

    public class Encounter : ModBehavior
    {
        public Encounter(ModMerging merging)
        {
            Delay = 0;
            Duration = EncounterEnd;
            Merging = merging;
        }

        public Encounter()
            : this(ModMerging.Add) { }
    }

    public class Turn : ModBehavior
    {
        public Turn(int delay, int duration, ModMerging merging)
        {
            Delay = delay;
            Duration = duration;
            Merging = merging;
        }

        public Turn()
            : this(0, 1, ModMerging.Add) { }

        public Turn(ModMerging merging)
            : this(0, 1, merging) { }


        public Turn(int duration)
            : this(0, duration, ModMerging.Add) { }
    }

    public class ExpireOnZero : ModBehavior
    {
        public ExpireOnZero()
        {
            Merging = ModMerging.Combine;
        }

        public override void SetExpiry(RpgGraph graph, Mod? mod = null)
        {
            var res = graph.CalculatePropValue(mod!, x => x.Behavior is ExpireOnZero);
            Expiry = res == Dice.Zero
                ? ModExpiry.Expired
                : ModExpiry.Active;
        }
    }
}
