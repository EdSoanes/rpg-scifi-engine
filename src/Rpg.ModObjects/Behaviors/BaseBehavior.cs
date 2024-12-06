using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;

namespace Rpg.ModObjects.Behaviors
{
    public abstract class BaseBehavior
    {
        [JsonIgnore] public ModScope Scope { get; internal set; } = ModScope.Standard;

        public virtual void OnAdding(RpgGraph graph, Prop modProp, Mod mod)
        {
            //Don't add if the source is a Value without a ValueFunction and the Value = null
            if (mod.Source != null || mod.SourceValue != null || mod.SourceValueFunc != null)
                modProp.Add(mod);

            var scopedMods = CreateScopedMods(graph, mod);
            if (scopedMods.Any())
                graph.AddMods(scopedMods);
        }

        public virtual void OnUpdating(RpgGraph graph, Mod mod)
        {
            var scopedMods = CreateScopedMods(graph, mod);
            if (scopedMods.Any())
                graph.AddMods(scopedMods);
        }

        protected virtual Mod[] CreateScopedMods(RpgGraph graph, Mod mod)
        {
            var scopedMods = new List<Mod>();
            if (Scope != ModScope.Standard)
            {
                var entities = graph.GetObjectsByScope(mod.EntityId, Scope);
                foreach (var entity in entities.Where(x => x.Props.ContainsKey(mod.Prop)))
                {
                    var ownerMods = ModFilters.ActiveByOwner(entity.GetMods(mod.Prop), mod.OwnerId);
                    if (!ownerMods.Any())
                    {
                        var syncedMod = new Permanent(mod.Id)
                            .Set(new PropRef(entity.Id, mod.Prop), mod);

                        scopedMods.Add(syncedMod);
                    }
                }
            }

            return scopedMods.ToArray();
        }

        protected Mod[] GetMatchingMods<T>(RpgGraph graph, Mod mod)
            where T : BaseBehavior
        {
            var rpgObj = graph.GetObject(mod.Target.EntityId);
            return rpgObj != null
                ? ModFilters.ActiveMatching<T>(rpgObj.GetMods(), mod).ToArray()
                : [];
        }

        protected virtual void RemoveScopedMods(RpgGraph graph, Mod mod)
        {
            var scopedMods = new List<Mod>();
            if (Scope != ModScope.Standard)
            {
                var entities = graph.GetObjectsByScope(mod.EntityId, Scope);
                foreach (var entity in entities.Where(x => x.Props.ContainsKey(mod.Prop)))
                {
                    var existing = entity.GetMods(mod.Prop).Where(x => x.OwnerId == mod.Id);
                    if (existing.Any())
                        entity.RemoveMods(existing.ToArray());
                }
            }
        }
    }
}
