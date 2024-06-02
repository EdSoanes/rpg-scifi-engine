using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Modifiers
{
    public abstract class ModBehavior
    {
        [JsonProperty] public ModType Type { get; protected set; } = ModType.Standard;
        [JsonProperty] public ModMerging Merging { get; protected set; } = ModMerging.Add;
        [JsonProperty] public ModScope Scope { get; protected set; } = ModScope.Standard;
        [JsonProperty] public ModExpiry Expiry { get; private set; } = ModExpiry.Active;

        public virtual void OnAdding(RpgGraph graph, PropStore propStore, Mod mod)
        { }

        public virtual void OnUpdating(RpgGraph graph, PropStore propStore, Mod mod)
        { }

        public virtual bool OnRemoving(RpgGraph graph, Mod mod)
            => false;

        //public int StartTurn
        //{
        //    get
        //    {
        //        if (Duration == TimeTypes.ExpiredDuration)
        //            return TimeTypes.ExpiredDuration;

        //        if (Duration == TimeTypes.PermanentDuration)
        //            return int.MinValue;

        //        if (Duration == TimeTypes.EncounterDuration)
        //            return 1;

        //        return TurnAdded + Delay;
        //    }
        //}

        //public int EndTurn
        //{
        //    get
        //    {
        //        if (Duration == TimeTypes.ExpiredDuration)
        //            return TimeTypes.ExpiredDuration;

        //        if (Duration == TimeTypes.PermanentDuration)
        //            return TimeTypes.PermanentDuration;

        //        if (Duration == TimeTypes.EncounterDuration)
        //            return TimeTypes.EncounterDuration;

        //        return StartTurn + Duration - 1;
        //    }
        //}

        //public void OnAdd(int turn)
        //    => TurnAdded = turn;

        //public void SyncWith(ModBehavior behavior)
        //{
        //    Delay = behavior.Delay;
        //    Duration = behavior.Duration;
        //    TurnAdded = behavior.TurnAdded;
        //}

        //public void SetExpired()
        //{
        //    Delay = 0;
        //    Duration = TimeTypes.ExpiredDuration;
        //}

        //public virtual void OnAdding(RpgGraph graph, Mod? mod = null)
        //{
        //    TurnAdded = graph.Time.Current.Tick;
        //}

        //public virtual ModExpiry OnUpdating(RpgGraph graph, Time.Time time, Mod? mod = null)
        //{
        //    if (Duration < TimeTypes.PermanentDuration)
        //    {
        //        if (time.Type == TimeTypes.EncounterStart || time.Type == TimeTypes.EncounterEnd)
        //            Expiry = ModExpiry.Remove;
        //        else if (EndTurn < time.Tick)
        //            Expiry = ModExpiry.Expired;
        //        else if (StartTurn > time.Tick)
        //            Expiry = ModExpiry.Pending;
        //        else
        //            Expiry = ModExpiry.Active;
        //    }

        //    return Expiry;
        //}

        public ModBehavior Clone<T>(ModScope scope = ModScope.Standard)
            where T : ModBehavior
        {
            var behavior = Activator.CreateInstance<T>();
            //behavior.Delay = Delay;
            //behavior.Duration = Duration;
            //behavior.Expiry = Expiry;
            behavior.Merging = Merging;
            behavior.Scope = scope;
            behavior.Type = Type;
            //behavior.TurnAdded = TurnAdded;

            return behavior;
        }
    }

    public class Replace : ModBehavior
    {
        public Replace(ModType modType)
        {
            Merging = ModMerging.Replace;
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, PropStore propStore, Mod mod)
        {
            base.OnAdding(graph, propStore, mod);
            var modProp = propStore[mod.Prop]!;
            var oldMods = modProp.Get(mod.Behavior.Type, mod.Name);

            foreach (var oldMod in oldMods)
                modProp.Remove(oldMod);

            //Don't add if the source is a Value without a ValueFunction and the Value = null
            if (mod.SourcePropRef != null || mod.SourceValue != null || mod.SourceValueFunc.IsCalc)
                modProp.Add(mod);
        }
    }

    public class Combine : ModBehavior
    {
        public Combine(ModType modType)
        {
            Merging = ModMerging.Combine;
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, PropStore propStore, Mod mod)
        {
            var entity = graph.GetEntity(mod.EntityId);
            var oldValue = graph.CalculatePropValue(entity, mod.Prop, x => x.Behavior.Type == mod.Behavior.Type && x.Name == mod.Name);
            var newValue = (oldValue ?? Dice.Zero) + graph?.CalculateModValue(mod) ?? Dice.Zero;

            mod.SetSource(newValue);

            var modProp = propStore[mod.Prop]!;
            var oldMods = modProp.Get(mod.Behavior.Type, mod.Name);

            foreach (var oldMod in oldMods)
                modProp.Remove(oldMod);

            if (mod.SourceValue != null && mod.SourceValue != Dice.Zero)
                modProp.Add(mod);
        }
    }

    public class Add : ModBehavior
    {
        public Add(ModType modType)
        {
            Merging = ModMerging.Add;
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, PropStore propStore, Mod mod)
        {
            var modProp = propStore[mod.Prop]!;
            if (!modProp.Contains(mod))
                modProp.Mods.Add(mod);
        }
    }

    //public class Initial : ModBehavior
    //{
    //    public Initial()
    //    {
    //        Merging = ModMerging.Replace;
    //        Type = ModType.Initial;
    //    }
    //}

    //public class Base : ModBehavior
    //{
    //    public Base()
    //    {
    //        Merging = ModMerging.Replace;
    //        Type = ModType.Base;
    //    }
    //}

    //public class Override : ModBehavior
    //{
    //    public Override()
    //    {
    //        Merging = ModMerging.Replace;
    //        Type = ModType.Override;
    //    }
    //}

    //public class State : ModBehavior
    //{
    //    public State(int delay, int duration)
    //    {
    //        Merging = ModMerging.Combine;
    //        Type = ModType.State;
    //        Delay = delay;
    //        Duration = duration;
    //    }

    //    public State()
    //    {
    //        Merging = ModMerging.Combine;
    //        Type = ModType.State;
    //    }

    //    public State(int duration)
    //        : this(0, duration) { }

    //    public override ModExpiry OnUpdating(RpgGraph graph, Time.Time time, Mod? mod = null)
    //    {
    //        var res = graph.CalculateModValue(mod);
    //        Expiry = res == Dice.Zero
    //            ? ModExpiry.Expired
    //            : ModExpiry.Active;

    //        return Expiry;
    //    }
    //}

    //public class ForceState : ModBehavior
    //{
    //    public ForceState()
    //    {
    //        Merging = ModMerging.Replace;
    //        Type = ModType.ForceState;
    //    }
    //}

    //public class Permanent : ModBehavior
    //{
    //    public Permanent() { }

    //    public Permanent(ModScope scope = ModScope.Standard)
    //        => Scope = scope;
    //}

    //public class Synced : ModBehavior
    //{
    //    public Synced()
    //    {
    //        Type = ModType.Synced;
    //    }
    //}

    //public class Encounter : ModBehavior
    //{
    //    public Encounter(ModMerging merging)
    //    {
    //        Delay = 0;
    //        Duration = TimeTypes.AsTurn(TimeTypes.EncounterEnd);
    //        Merging = merging;
    //    }

    //    public Encounter()
    //        : this(ModMerging.Add) { }
    //}

    //public class Turn : ModBehavior
    //{
    //    public Turn(int delay, int duration, ModMerging merging)
    //    {
    //        Delay = delay;
    //        Duration = duration;
    //        Merging = merging;
    //    }

    //    public Turn()
    //        : this(0, 1, ModMerging.Add) { }

    //    public Turn(ModMerging merging)
    //        : this(0, 1, merging) { }


    //    public Turn(int duration)
    //        : this(0, duration, ModMerging.Add) { }
    //}

    //public class ExpireOnZero : ModBehavior
    //{
    //    public ExpireOnZero()
    //    {
    //        Merging = ModMerging.Combine;
    //    }

    //    public override ModExpiry OnUpdating(RpgGraph graph, Time.Time time, Mod? mod = null)
    //    {
    //        var res = graph.CalculatePropValue(mod!, x => x.Behavior is ExpireOnZero);
    //        Expiry = res == Dice.Zero
    //            ? ModExpiry.Expired
    //            : ModExpiry.Active;

    //        return Expiry;
    //    }
    //}
}
