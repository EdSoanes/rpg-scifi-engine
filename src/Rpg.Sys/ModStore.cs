using Rpg.Sys.Modifiers;
using System.Collections;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.Sys
{
    public class ModStore : IDictionary<string, ModProp>
    {
        private Graph? _graph;

        private bool _restoring = false;
        private bool Restoring { get => _restoring || _graph?.Entities == null; }

        private readonly Dictionary<Guid, Dictionary<string, ModProp>> _store = new Dictionary<Guid, Dictionary<string, ModProp>>();

        public ModProp this[string key]
        {
            get
            {
                TryGetValue(key, out var res);
                return res;
            }

            set => Add(key, value);
        }

        public ICollection<string> Keys => AllKeys();

        public ICollection<ModProp> Values => AllValues();

        public int Count => _store.Sum(x => x.Value.Sum(y => y.Value.Modifiers.Count()));

        public bool IsReadOnly => false;

        public IEnumerable<Modifier> FindForModifierSet(Guid artifactId, string? modifierSet = null)
        {
            return _graph!.Mods.FindMods(mod =>
            {
                var stateMod = mod as StateModifier;
                if (stateMod == null)
                    return false;

                return stateMod.ModifierType == ModifierType.State
                    && stateMod.ArtifactId == artifactId
                    && (modifierSet == null || stateMod.ModifierSet == modifierSet);
            });
        }

        public IEnumerable<Modifier> FindMods(Func<Modifier?, bool>? filter = null)
        {
            var allMods = _store.SelectMany(x => x.Value.Values.SelectMany(y => y.Modifiers));
            foreach (var mod in allMods)
            {
                if (filter == null || filter(mod))
                    yield return mod;
            }
        }

        public List<Modifier>? GetMods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => Get(PropRef.FromPath(entity, expression, true))?.Modifiers;

        public ModProp? Get<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => Get(PropRef.FromPath(entity, expression, true));

        public void Initialize(Graph graph)
        {
            _graph = graph;
            GraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        public Dice BaseValue<TEntity, T1>(TEntity entity, Expression<Func<TEntity, T1>> propExpr)
            where TEntity : ModdableObject
        {
            var propRef = PropRef.FromPath(entity, propExpr, true);
            return Get(propRef)?.BaseValue ?? Dice.Zero;
        }

        public ModProp? Get(string key) => this[key];

        public ModProp? Get(PropRef propRef)
        {
            return propRef.Id != null
                ? Get(propRef.Id!.Value, propRef.Prop!)
                : null;
        }

        public ModProp? Get(Guid? entityId, string? prop)
        {
            if (entityId == null  || string.IsNullOrEmpty(prop))
                return null;

            return TryGetValue(MakeKey(entityId.Value, prop), out var res)
                ? res
                : null;
        }

        public void Add(IEnumerable<ModdableObject> entities)
        {
            foreach (var entity in entities)
            {
                foreach (var propInfo in entity.ModdableProperties())
                {
                    var modProp = Get(entity.Id, propInfo.Name);
                    if (modProp == null)
                    {
                        modProp = new ModProp(entity.Id, propInfo.Name, propInfo.PropertyType.Name);
                        _Add(modProp);
                    }
                }
            }

            //Execute in reverse order to set up child entities first so
            // parent entity mods on children can override child entity mods
            var mods = entities
                .Reverse()
                .SelectMany(x => x.OnSetup())
                .ToArray();

            Add(mods);
        }

        public void Add(params Modifier[] mods)
        {
            var updated = new List<ModProp>();
            foreach (var mod in mods)
            {
                mod.OnAdd(_graph!.Turn);

                var modProp = Get(mod.Target.Id, mod.Target.Prop);
                if (modProp == null)
                    throw new Exception($"Missing ModProp for {mod.Target}");

                if (modProp.Add(mod) && !updated.Any(x => x.EntityId == modProp.EntityId && x.Prop == modProp.Prop))
                    updated.Add(modProp);
            }

            if (updated.Any())
                NotifyPropertiesChanged(updated);
        }

        public void Add(string key, ModProp value)
        {
            _Add(value);
            NotifyPropertyChanged(value);
        }

        private void _Add(ModProp value)
        {
            if (!_store.ContainsKey(value.EntityId))
                _store.Add(value.EntityId, new Dictionary<string, ModProp>());
            else
                _store[value.EntityId].Remove(MakeKey(value.EntityId, value.Prop));

            _store[value.EntityId].Add(value.Prop, value);
        }

        public void Add(KeyValuePair<string, ModProp> item)
        {
            _Add(item.Value);
            NotifyPropertyChanged(item.Value);
        }

        public void Restore(ModStore store)
        {
            _restoring = true;

            Clear();
            foreach (var item in store.AllValues())
                Add("", item);

            _restoring = false;

            PropertyChanged(AllValues());
        }

        public void Clear() => _store.Clear();

        public void Clear(Guid entityId)
        {
            if (!_store.ContainsKey(entityId))
                return;

            var entityMods = _store[entityId];
            foreach (var prop in entityMods.Keys)
            {
                var modProp = Get(entityId, prop);
                modProp?.Clear();
            }
        }

        public bool Contains(KeyValuePair<string, ModProp> item) => ContainsKey(item.Key);

        public bool ContainsKey(string key)
        {
            if (!TryParseKey(key, out var res))
                return false;

            return _store.ContainsKey(res.Item1) && _store[res.Item1].ContainsKey(res.Item2);
        }

        public void CopyTo(KeyValuePair<string, ModProp>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, ModProp>> GetEnumerator()
        {
            foreach (var id in _store.Keys)
                foreach (var prop in _store[id].Keys)
                    yield return new KeyValuePair<string, ModProp>(MakeKey(id, prop), _store[id][prop]);
        }

        public bool Remove(Modifier mod)
        {
            var modified = ModOperation(mod, (modProp) =>
            {
                var removed = modProp.Remove(mod);
                if (removed != null)
                {
                    NotifyPropertyChanged(modProp);
                    return true;
                }

                return false;
            });

            return modified;
        }

        public bool Remove(Guid entityId)
        {
            var res = false;

            if (_store.ContainsKey(entityId))
            {
                var modProps = _store[entityId];
                foreach (var prop in modProps.Values.Select(x => x.Prop).ToList())
                {
                    Remove(entityId, prop);
                    res = true;
                }
            }

            //Remove state mods
            var stateMods = FindForModifierSet(entityId).ToList();
            foreach (var mod in stateMods)
            {
                Remove(mod);
                res = true;
            }

            return res;
        }

        public bool Expire(params Modifier[] mods)
        {
            var updates = new List<ModProp>();
            foreach (var mod in mods)
            {
                var updated = ModOperation(mod, (modProp) =>
                {
                    mod.Duration.Expire(_graph.Turn);
                    return modProp;
                });

                if (updated != null)
                    updates.Add(updated);
            }

            if (updates.Any())
                NotifyPropertiesChanged(updates);

            return updates.Any();
        }

        public bool Remove(PropRef? moddableProperty)
            => Remove(moddableProperty?.Id, moddableProperty?.Prop);

        public bool Remove(Guid? entityId, string? prop, ModifierType modifierType) 
            => RemoveByFilter(entityId, prop, (mod) => mod.ModifierType == modifierType);

        public bool Remove(Guid? entityId, string? prop)
            => RemoveByFilter(entityId, prop, null);

        private bool RemoveByFilter(Guid? entityId, string? prop, Func<Modifier, bool>? filter)
        {
            bool res = false;

            var modProp = Get(entityId, prop);
            if (modProp != null)
            {
                if (modProp.Remove(filter).Any())
                    NotifyPropertyChanged(modProp);
            }

            return res;
        }

        public bool Remove(string key)
        {
            return TryParseKey(key, out var res)
                ? Remove(res.Item1, res.Item2)
                : false;
        }

        public bool UpdateOnTurn(int newTurn)
        {
            var res = false;
            foreach (var modProp in this.Select(x => x.Value))
            {
                var updated = modProp.Update(newTurn);
                if (updated.Any())
                {
                    res = true;
                    PropertyChanged(modProp);
                }
            }

            return res;
        }

        public bool Remove(KeyValuePair<string, ModProp> item) => Remove(item.Key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out ModProp value)
        {
            if (TryParseKey(key, out (Guid, string) res))
            {
                Dictionary<string, ModProp> entityMods;
                if (_store.ContainsKey(res.Item1))
                {
                    entityMods = _store[res.Item1];
                    if (entityMods.ContainsKey(res.Item2))
                    {
                        value = entityMods[res.Item2];
                        return true;
                    }
                }
            }
    
            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public (Guid, string) EmptyKey => (Guid.Empty, string.Empty);

        private bool TryParseKey(string key, out (Guid, string) res)
        {
            if (string.IsNullOrEmpty(key))
            {
                res = EmptyKey;
                return false;
            }

            var parts = key.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var id))
            {
                res = EmptyKey;
                return false;
            }

            res = (id, parts[1]);
            return true;
        }

        private string MakeKey(Guid id, string prop) => $"{id}.{prop}";

        private string[] AllKeys()
        {
            var res = new List<string>();
            foreach (var id in _store.Keys)
                foreach (var prop in _store[id].Keys)
                    res.Add(MakeKey(id, prop));
            return res.ToArray();
        }

        private ICollection<ModProp> AllValues()
        {
            var res = new List<ModProp>();
            foreach (var id in _store.Keys)
            {
                foreach (var prop in _store[id].Keys)
                    res.Add(_store[id][prop]);
            }
            return res.ToArray();
        }

        public void NotifyPropertiesChanged(IEnumerable<ModProp> modProps)
        {
            if (!Restoring)
            {
                var affected = new List<ModProp>();
                foreach (var modProp in modProps)
                    affected.AddRange(GetAffectedModProps(modProp));

                affected = affected
                    .GroupBy(x => $"{x.EntityId}.{x.Prop}")
                    .Select(x => x.First())
                    .ToList();

                foreach (var modProp in affected)
                    modProp.Evaluate(_graph!);

                PropertyChanged(affected);
            }
        }

        public void NotifyPropertyChanged(ModProp modProp)
        {
            if (!Restoring)
            {
                var modProps = GetAffectedModProps(modProp);
                PropertyChanged(modProps);
            }
        }

        public void NotifyPropertyChanged(Guid id, string prop)
        {
            if (!Restoring)
            {
                var modProp = Get(id, prop);
                if (modProp != null)
                {
                    var modProps = GetAffectedModProps(modProp);
                    PropertyChanged(modProps);
                }
                else
                {
                    var modProps = GetAffectedModProps(id, prop);
                    PropertyChanged(id, prop);
                    PropertyChanged(modProps);
                }
            }
        }

        private void PropertyChanged(Guid id, string prop)
        {
            if (!Restoring)
            {
                var entity = _graph!.Entities.Get(id);
                entity?.CallPropertyChanged(prop);
            }
        }

        private void PropertyChanged(ModProp? modProp)
            => PropertyChanged(new[] { modProp });

        private void PropertyChanged(IEnumerable<ModProp?> modProps)
        {
            if (!Restoring)
            {
                foreach (var mp in modProps.Where(x => x != null).GroupBy(x => x!.EntityId))
                {
                    var entity = _graph!.Entities.Get(mp.Key);
                    foreach (var p in mp)
                    {
                        p.Evaluate(_graph);
                        entity?.SetModdableProperty(p.Prop, p.Value);
                    }
                }
            }
        }

        private List<ModProp> GetAffectedModProps(ModProp modProp)
        {
            var res = new List<ModProp>
            {
                modProp
            };

            res.AddRange(GetAffectedModProps(modProp.EntityId, modProp.Prop));

            return res;
        }

        private List<ModProp> GetAffectedModProps(Guid id, string prop)
        {
            var res = new List<ModProp>();

            var modProps = AllValues().Where(x => x.AffectedBy(id, prop));
            foreach (var mp in modProps)
                res.AddRange(GetAffectedModProps(mp));

            return res;
        }

        private T? ModOperation<T>(Modifier mod, Func<ModProp, T> op)
        {
            if (_store.ContainsKey(mod.Target.Id!.Value))
            {
                var entityMods = _store[mod.Target.Id!.Value];
                if (entityMods.ContainsKey(mod.Target.Prop))
                {
                    var modProp = entityMods[mod.Target.Prop];
                    if (modProp != null)
                        return op.Invoke(modProp);
                }
            }

            return default;
        }
    }
}
