using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class ModStore : IDictionary<string, MetaModdableProperty>
    {
        private readonly Dictionary<Guid, Dictionary<string, MetaModdableProperty>> _store = new Dictionary<Guid, Dictionary<string, MetaModdableProperty>>();
        [JsonIgnore] protected IPropEvaluator? PropEvaluator { get; set; }

        public MetaModdableProperty this[string key]
        {
            get
            {
                TryGetValue(key, out var res);
                return res;
            }

            set => Add(key, value);
        }

        public ICollection<string> Keys => AllKeys();

        public ICollection<MetaModdableProperty> Values => AllValues();

        public int Count => _store.Sum(x => x.Value.Sum(y => y.Value.Modifiers.Count()));

        public bool IsReadOnly => false;

        public void Initialize(IPropEvaluator propEvaluator)
        {
            PropEvaluator = propEvaluator;
        }

        public MetaModdableProperty? Get(PropRef? moddableProperty, bool createMissingEntries = false)
        {
            return moddableProperty?.Id != null
                ? Get(moddableProperty.Id.Value, moddableProperty.Prop!, createMissingEntries)
                : null;
        }

        public MetaModdableProperty? Get(Guid? entityId, string? prop, bool createMissingEntries = false)
        {
            if (entityId == null  || string.IsNullOrEmpty(prop))
                return null;

            if (createMissingEntries)
            {
                if (!_store.ContainsKey(entityId.Value))
                    _store.Add(entityId.Value, new Dictionary<string, MetaModdableProperty>());

                var entityMods = _store[entityId.Value];
                if (!entityMods.ContainsKey(prop))
                    entityMods.Add(prop, new MetaModdableProperty(entityId.Value, prop));

                return entityMods[prop];
            }
            else
            {
                return TryGetValue(MakeKey(entityId.Value, prop), out var res)
                    ? res
                    : null;
            }
        }

        public void Add(Modifier[] mods)
        {
            foreach (var mod in mods)
                Add(mod);
        }

        public void Add(string key, MetaModdableProperty value)
        {
            foreach (var mod in value.Modifiers)
                Add(mod);
        }

        public void Add(KeyValuePair<string, MetaModdableProperty> item) => Add(item.Key, item.Value);

        public void Add(Modifier mod)
        {
            var modProp = Get(mod.Target, true)!;
            if (mod.ModifierAction == ModifierAction.Accumulate)
            {
                modProp.Modifiers.Add(mod);
            }
            else if (mod.ModifierAction == ModifierAction.Sum)
            {
                var modDice = PropEvaluator!.Evaluate(modProp.MatchingMods(mod)) + PropEvaluator!.Evaluate(new[] { mod });
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
        public bool Contains(KeyValuePair<string, MetaModdableProperty> item) => ContainsKey(item.Key);

        public bool ContainsKey(string key)
        {
            if (!TryParseKey(key, out var res))
                return false;

            return _store.ContainsKey(res.Item1) && _store[res.Item1].ContainsKey(res.Item2);
        }

        public void CopyTo(KeyValuePair<string, MetaModdableProperty>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, MetaModdableProperty>> GetEnumerator()
        {
            foreach (var id in _store.Keys)
                foreach (var prop in _store[id].Keys)
                    yield return new KeyValuePair<string, MetaModdableProperty>(MakeKey(id, prop), _store[id][prop]);
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

            return _store.Remove(entityId);
        }

        public bool Remove(PropRef? moddableProperty)
            => Remove(moddableProperty?.Id, moddableProperty?.Prop);

        public bool Remove(Guid? entityId, string? prop)
        {
            var modProp = Get(entityId, prop);
            if (modProp != null)
            {
                var removed = modProp.Modifiers.Count > 0;
                modProp.Modifiers.Clear();

                return removed;
            }

            return false;
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
                    res = true;
            }

            return res;
        }

        public bool Remove(KeyValuePair<string, MetaModdableProperty> item) => Remove(item.Key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out MetaModdableProperty value)
        {
            if (TryParseKey(key, out (Guid, string) res))
            {
                Dictionary<string, MetaModdableProperty> entityMods;
                if (_store.ContainsKey(res.Item1))
                {
                    entityMods = _store[res.Item1];
                }
                else
                {
                    entityMods = new Dictionary<string, MetaModdableProperty>();
                    _store.Add(res.Item1, entityMods);
                }

                if (entityMods.ContainsKey(res.Item2))
                {
                    value = entityMods[res.Item2];
                    return true;
                }
                else
                {
                    value = new MetaModdableProperty(res.Item1, res.Item2);
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

        private ICollection<MetaModdableProperty> AllValues()
        {
            var res = new List<MetaModdableProperty>();
            foreach (var id in _store.Keys)
            {
                foreach (var prop in _store[id].Keys)
                    res.Add(_store[id][prop]);
            }
            return res.ToArray();
        }
    }
}
