using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Behaviors
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
}
