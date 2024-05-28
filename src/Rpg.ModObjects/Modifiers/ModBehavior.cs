using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Modifiers
{
    public enum ModMerging
    {
        Add,
        Combine,
        Replace
    }

    public enum ModType
    {
        Initial,
        Base,
        Override,
        Standard,
        State,
        ForceState,
        Synced
    }

    public abstract class ModBehavior
    {
        [JsonProperty] public ModType Type { get; protected set; } = ModType.Standard;
        [JsonProperty] public ModMerging Merging { get; protected set; } = ModMerging.Add;
        [JsonProperty] public int Delay { get; protected set; } = int.MinValue;
        [JsonProperty] public int Duration { get; protected set; } = int.MaxValue;
        [JsonProperty] public int TurnAdded { get; protected set; }

        public int StartTurn { get => Delay == int.MinValue ? Delay : TurnAdded + Delay; }
        public int EndTurn { get => Duration >= int.MaxValue - 2 ? Duration : StartTurn + Duration - 1; }

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
            Delay = int.MinValue;
            Duration = int.MinValue;
        }

        public bool CanRemove(ModGraph graph, Mod mod)
        {
            var expiry = GetExpiry(graph, mod);
            return expiry == ModExpiry.Expired && (graph.Turn == 0 || graph.Turn == int.MaxValue - 1);
        }

        public virtual ModExpiry GetExpiry(ModGraph graph, Mod? mod = null)
        {
            if (EndTurn < graph.Turn)
                return ModExpiry.Expired;

            if (StartTurn > graph.Turn)
                return ModExpiry.Pending;

            return ModExpiry.Active;
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

        public override ModExpiry GetExpiry(ModGraph graph, Mod? mod = null)
        {
            var res = graph.CalculateModValue(mod);
            return res == Dice.Zero
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
            Delay = int.MinValue + 1;
            Duration = int.MaxValue - 2;
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

        public override ModExpiry GetExpiry(ModGraph graph, Mod? mod = null)
        {
            var res = graph.CalculatePropValue(mod!, x => x.Behavior is ExpireOnZero);
            return res == Dice.Zero
                ? ModExpiry.Expired
                : ModExpiry.Active;
        }
    }
}
