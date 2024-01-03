using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class ModStore : IDictionary<string, ModProp>
    {
        private EntityGraph? _graph;
        
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

        public void Initialize(EntityGraph graph)
        {
            _graph = graph;
        }

        public List<Modifier>? GetMods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => Get(PropRef.FromPath(entity, expression, true))?.Modifiers;

        public ModProp? Get<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => Get(PropRef.FromPath(entity, expression, true));

        public ModProp? Get(PropRef propRef, bool createMissingEntries = false)
        {
            return propRef.Id != null
                ? Get(propRef.Id!.Value, propRef.Prop!, createMissingEntries)
                : null;
        }

        public ModProp? Get(Guid? entityId, string? prop, bool createMissingEntries = false)
        {
            if (entityId == null  || string.IsNullOrEmpty(prop))
                return null;

            if (createMissingEntries)
            {
                if (!_store.ContainsKey(entityId.Value))
                    _store.Add(entityId.Value, new Dictionary<string, ModProp>());

                var entityMods = _store[entityId.Value];
                if (!entityMods.ContainsKey(prop))
                    entityMods.Add(prop, new ModProp(entityId.Value, prop));

                return entityMods[prop];
            }
            else
            {
                return TryGetValue(MakeKey(entityId.Value, prop), out var res)
                    ? res
                    : null;
            }
        }

        public void Add(params Modifier[] mods)
        {
            foreach (var mod in mods)
                Add(mod);
        }

        public void Add(string key, ModProp value)
        {
            foreach (var mod in value.Modifiers)
                Add(mod);
        }

        public void Add(KeyValuePair<string, ModProp> item) => Add(item.Key, item.Value);

        public void Add(Modifier mod)
        {
            var modProp = Get(mod.Target, true)!;
            if (mod.ModifierAction == ModifierAction.Accumulate)
            {
                modProp.Modifiers.Add(mod);
            }
            else if (mod.ModifierAction == ModifierAction.Sum)
            {
                var modDice = _graph!.Evaluator!.Evaluate(modProp.MatchingMods(mod)) + _graph!.Evaluator!.Evaluate(new[] { mod });
                modProp.RemoveMatchingMods(mod);
                if (modDice != Dice.Zero)
                {
                    mod.SetDice(modDice);
                    modProp.Modifiers.Add(mod);
                }
            }
            else if (mod.ModifierAction == ModifierAction.Replace)
            {
                modProp.RemoveMatchingMods(mod);
                modProp.Modifiers.Add(mod);
            }

            NotifyPropertyChanged(modProp);
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
                if (modProp != null)
                {
                    if (modProp.Modifiers.Any())
                    {
                        var toRemove = modProp.Modifiers.Where(x => x.CanBeCleared()).ToArray();
                        foreach (var remove in toRemove)
                            modProp.Modifiers.Remove(remove);
                    }
                }
            }
        }

        public bool Contains(Modifier mod) => Get(mod.Target.Id, mod.Target.Prop!)?.Modifiers.Any(x => x.Id == mod.Target.Id) ?? false;
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
            var res = false;

            if (_store.ContainsKey(mod.Target.Id!.Value))
            {
                var entityMods = _store[mod.Target.Id!.Value];
                if (!string.IsNullOrEmpty(mod.Target.Prop) && entityMods.ContainsKey(mod.Target.Prop))
                {
                    var modProp = entityMods[mod.Target.Prop];
                    var toRemove = modProp.Modifiers.FirstOrDefault(x => x.Id == mod.Id && mod.CanBeCleared());
                    if (toRemove != null)
                    {
                        modProp.Modifiers.Remove(toRemove);
                        NotifyPropertyChanged(modProp);
                        res = true;
                    }
                }
            }

            return res;
        }

        public bool Remove(Guid entityId)
        {
            if (!_store.ContainsKey(entityId))
                return false;

            var modProps = _store[entityId];
            foreach (var prop in modProps.Values.Select(x => x.Prop).ToList())
                Remove(entityId, prop);

            return true;
        }

        public bool Remove(PropRef? moddableProperty)
            => Remove(moddableProperty?.Id, moddableProperty?.Prop);

        public bool Remove(Guid? entityId, string? prop)
        {
            bool removed = false;

            var modProp = Get(entityId, prop);
            if (modProp != null)
            {
                modProp.Modifiers.Clear();
                var affectedProps = GetAffectedModProps(modProp);
                foreach (var group in affectedProps.GroupBy(x => x.Id))
                {
                    foreach (var mp in group)
                    {
                        var entity = _graph!.Entities!.Get(mp.Id);
                        var toRemove = mp.Modifiers.Where(x => x.Source.Prop == modProp.Prop).ToList();

                        foreach (var r in toRemove)
                        {
                            if (!r.DiceCalc.IsCalc)
                                mp.Modifiers.Remove(r);
                            else
                            {
                                var dice = _graph!.Evaluator!.Evaluate(r);
                                if (dice == Dice.Zero)
                                    mp.Modifiers.Remove(r);
                            }
                        }

                        removed = true;
                    }
                }

                PropertyChanged(affectedProps);
            }

            return removed;
        }

        public bool Remove(string key)
        {
            return TryParseKey(key, out var res)
                ? Remove(res.Item1, res.Item2)
                : false;
        }

        public bool Remove(int currentTurn)
        {
            var res = false;
            foreach (var modProp in  this.Select(x => x.Value))
            {
                var toRemove = new List<Modifier>();

                foreach (var mod in modProp.Modifiers)
                {
                    if (mod.ShouldBeRemoved(currentTurn))
                        toRemove.Add(mod);
                }

                foreach (var mod in toRemove)
                    modProp.Modifiers.Remove(mod);

                if (toRemove.Any())
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
                }
                else
                {
                    entityMods = new Dictionary<string, ModProp>();
                    _store.Add(res.Item1, entityMods);
                }

                if (entityMods.ContainsKey(res.Item2))
                {
                    value = entityMods[res.Item2];
                    return true;
                }
                else
                {
                    value = new ModProp(res.Item1, res.Item2);
                    entityMods.Add(res.Item2, value);
                    
                    return true;
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

        public void NotifyPropertyChanged(ModProp modProp)
        {
            var modProps = GetAffectedModProps(modProp);
            PropertyChanged(modProps);
        }

        public void NotifyPropertyChanged(Guid id, string prop)
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
                PropertyChanged(modProps);
            }
        }

        private void PropertyChanged(IEnumerable<ModProp> modProps)
        {
            foreach (var mp in modProps.GroupBy(x => x.Id))
            {
                var entity = _graph!.Entities!.Get(mp.Key);
                foreach (var p in mp)
                    entity?.PropChanged(p.Prop);
            }
        }

        private void PropertyChanged(ModProp? modProp)
        {
            if (modProp != null)
                PropertyChanged(new[] { modProp });
        }

        private List<ModProp> GetAffectedModProps(ModProp modProp)
        {
            var res = new List<ModProp>();

            if (_graph != null && modProp != null)
            {
                res.Add(modProp);
                res.AddRange(GetAffectedModProps(modProp.Id, modProp.Prop));
            }

            return res;
        }

        private List<ModProp> GetAffectedModProps(Guid id, string prop)
        {
            var res = new List<ModProp>();

            if (_graph != null)
            {
                var modProps = AllValues()
                    .Where(x => x.Modifiers.Any(m => m.Source.Id == id && m.Source.Prop == prop));

                foreach (var mp in modProps)
                    res.AddRange(GetAffectedModProps(mp));
            }

            return res;
        }
    }
}
