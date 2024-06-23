using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Mods
{
    public abstract class BaseBehavior : IBehavior
    {
        [JsonProperty] public ModType Type { get; protected set; } = ModType.Standard;
        [JsonProperty] public ModScope Scope { get; internal set; } = ModScope.Standard;
        [JsonProperty] public LifecycleExpiry Expiry { get; private set; } = LifecycleExpiry.Active;

        public virtual void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            if (!modProp.Contains(mod))
                modProp.Add(mod);

            OnScoping(graph, modProp, mod);
        }

        public virtual void OnScoping(RpgGraph graph, Prop modProp, Mod mod)
        {
            if (Scope != ModScope.Standard)
            {
                var entities = graph.GetScopedEntities(mod.EntityId, Scope);
                foreach (var entity in entities)
                {
                    var syncedMod = new SyncedMod(mod.Id)
                        .SetProps(new PropRef(entity.Id, mod.Prop), mod)
                        .Create(mod.Name);

                    graph.AddMods(syncedMod);
                }
            }
        }

        public virtual void OnUpdating(RpgGraph graph, Prop modProp, Mod mod)
        {
            OnScoping(graph, modProp, mod);
        }

        public virtual bool OnRemoving(RpgGraph graph, Prop modProp, Mod mod)
        {
            //TODO: Handle scope here...
            return false;
        }

        public BaseBehavior Clone<T>(ModScope scope = ModScope.Standard)
            where T : BaseBehavior
        {
            var behavior = Activator.CreateInstance<T>();

            behavior.Scope = scope;
            behavior.Type = Type;

            return behavior;
        }
    }

    public class Replace : BaseBehavior
    {
        public Replace(ModType modType)
        {
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            base.OnAdding(graph, modProp, mod);
            var oldMods = modProp.Get(mod.Behavior.Type, mod.Name);

            foreach (var oldMod in oldMods)
                modProp.Remove(oldMod);

            //Don't add if the source is a Value without a ValueFunction and the Value = null
            if (mod.SourcePropRef != null || mod.SourceValue != null || mod.SourceValueFunc.IsCalc)
            {
                modProp.Add(mod);
                OnScoping(graph, modProp, mod);
            }
        }
    }

    public class Combine : BaseBehavior
    {
        public Combine(ModType modType)
        {
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            var matchingMods = MatchingMods(graph, mod);
            var oldValue = graph.CalculateModsValue(matchingMods);
            var newValue = oldValue + graph.CalculateModValue(mod);

            mod.SetSource(newValue);

            foreach (var matchingMod in matchingMods)
                modProp.Remove(matchingMod);

            if (mod.SourceValue != null && mod.SourceValue != Dice.Zero)
            {
                modProp.Add(mod);
                OnScoping(graph, modProp, mod);
            }
        }

        protected Mod[] MatchingMods(RpgGraph graph, Mod mod)
            => graph.GetMods(mod, x => x.Behavior is ExpiresOn && x.Behavior.Type == mod.Behavior.Type && x.Name == mod.Name);
    }

    public class ExpiresOn : Combine
    {
        [JsonProperty] public Dice Value { get; private set; }

        public ExpiresOn(int value)
            : base(ModType.Standard)
        { }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            var matchingMods = MatchingMods(graph, mod);
            var oldValue = graph.CalculateModsValue(matchingMods);
            var newValue = oldValue + graph.CalculateModValue(mod);

            mod.SetSource(newValue);

            foreach (var matchingMod in matchingMods)
                modProp.Remove(matchingMod);

            if (mod.SourceValue != null && mod.SourceValue != Value)
            {
                modProp.Add(mod);
                OnScoping(graph, modProp, mod);
            }
        }
    }

    public class Add : BaseBehavior
    {
        public Add(ModType modType)
        {
            Type = modType;
        }

        public override void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            if (!modProp.Contains(mod))
                modProp.Mods.Add(mod);
        }
    }

    public class State : Combine
    {
        public State()
            : base(ModType.State) 
        { }
    }

    public class ForceState : Combine
    {
        public ForceState()
            : base(ModType.ForceState)
        { }
    }
}
