using Newtonsoft.Json;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Moddable
{
    public class ModObjectPropStore
    {
        [JsonProperty] protected Guid EntityId { get; set; }
        [JsonProperty] protected Dictionary<string, ModObjectProp> ModObjProps { get; set; } = new Dictionary<string, ModObjectProp>();

        [JsonConstructor] private ModObjectPropStore() { }

        public ModObjectProp? this[string prop]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(prop))
                    return null;

                if (ModObjProps.ContainsKey(prop))
                    return ModObjProps[prop];

                return null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value?.Prop) && !ModObjProps.ContainsKey(prop))
                    ModObjProps.Add(prop, value);
            }
        }

        public ModObjectPropStore(Guid entityId)
            => EntityId = entityId;

        public ModObjectProp? Create(string prop)
        {
            var modObjectProp = this[prop];
            if (modObjectProp == null)
                this[prop] = new ModObjectProp(EntityId, prop);

            return this[prop];
        }

        public Dice? Evaluate(string prop, string? modifierName = null, ModifierType? modifierType = null)
            => Create(prop)
                ?.Evaluate(modifierName, modifierType);

        public void Add(ModObject rootEntity, params Modifier[] mods)
            => Add(rootEntity, true, mods);

        public void Init(ModObject rootEntity, params Modifier[] mods)
            => Add(rootEntity, false, mods);

        public void Remove(IEnumerable<Modifier> mods, Action<ModObject, string>? onAffected = null)
            => this.ForEach(mods.ToArray(), (entity, prop, propMods) =>
            {
                var removed = entity.PropStore[prop]
                    ?.Remove(propMods);

                if (removed != null && removed.Any())
                    onAffected?.Invoke(entity, prop);
            });


        private void Add(ModObject rootEntity, bool evaluate, params Modifier[] mods)
            => rootEntity.ForEach(mods, (entity, prop, propMods) =>
            {
                var propertyModifiers = entity.PropStore.Create(prop);
                propertyModifiers?.Add(entity, propMods);

                if (evaluate)
                    entity.SetModdableValue(prop);
            });

        public string[] Update()
        {
            var updatedProps = new List<string>();
            var turn = Graph.Current.Turn;

            foreach (var modObjProp in ModObjProps.Values)
            {
                var toRemove = new List<Modifier>();

                foreach (var mod in modObjProp.AllModifiers)
                {
                    mod.OnUpdate(turn);

                    var expiry = mod.Duration.GetExpiry(turn);
                    if (expiry == ModifierExpiry.Remove)
                    {
                        toRemove.Add(mod);
                        updatedProps.Add(modObjProp.Prop);
                    }
                    else if (expiry == ModifierExpiry.Expired)
                    {
                        mod.Duration.Expire(turn);
                        updatedProps.Add(modObjProp.Prop);
                    }
                    else if (expiry != mod.Duration.GetExpiry(turn - 1))
                    {
                        updatedProps.Add(modObjProp.Prop);
                    }
                }

                Remove(toRemove);
            }

            return updatedProps.Distinct().ToArray();
        }

        public IEnumerable<ModObjectPropRef> AffectedBy()
        {
            var res = new List<ModObjectPropRef>();
            foreach (var modObjRef in ModObjProps.Values)
                if ()
            return res;
        }

    }
}
