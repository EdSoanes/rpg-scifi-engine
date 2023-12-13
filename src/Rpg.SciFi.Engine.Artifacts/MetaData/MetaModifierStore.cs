using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaModifierStore : IDictionary<string, List<Modifier>>
    {
        private readonly Dictionary<Guid, Dictionary<string, List<Modifier>>> _store = new Dictionary<Guid, Dictionary<string, List<Modifier>>>();

        public List<Modifier> this[string key]
        {
            get
            {
                TryGetValue(key, out var res);
                return res ?? new List<Modifier>();
            }

            set => Add(key, value);
        }

        public ICollection<string> Keys => AllKeys();

        public ICollection<List<Modifier>> Values => AllValues();

        public int Count => _store.Sum(x => x.Value.Sum(y => y.Value.Count()));

        public bool IsReadOnly => false;

        public List<Modifier>? Get(ModdableProperty? moddableProperty)
        {
            return moddableProperty != null
                ? Get(moddableProperty.Id, moddableProperty.Prop!)
                : null;
        }

        public List<Modifier>? Get(Guid id, string prop)
        {
            return TryGetValue(MakeKey(id, prop), out var res)
                ? res
                : null;
        }

        public void Add(Modifier[] mods)
        {
            foreach (var mod in mods)
                Add(mod.IsBase());
        }

        public void Add(Modifier mod)
        {
            if (!string.IsNullOrEmpty(mod.Target.Prop))
            {
                if (!_store.ContainsKey(mod.Target.Id))
                    _store.Add(mod.Target.Id, new Dictionary<string, List<Modifier>>());

                var entityMods = _store[mod.Target.Id];
                if (!entityMods.ContainsKey(mod.Target.Prop))
                    entityMods.Add(mod.Target.Prop, new List<Modifier>());

                var mods = entityMods[mod.Target.Prop];
                if (!mods.Any(x => x.Id == mod.Id))
                    mods.Add(mod);
            }
        }

        public void Add(string key, List<Modifier> value)
        {
            if (TryParseKey(key, out (Guid, string) res))
            {
                if (!_store.ContainsKey(res.Item1))
                    _store.Add(res.Item1, new Dictionary<string, List<Modifier>>());

                var entityMods = _store[res.Item1];
                if (!entityMods.ContainsKey(res.Item2))
                    entityMods.Add(res.Item2, value);
                else
                    entityMods[res.Item2] = value;
            }
        }

        public void Add(KeyValuePair<string, List<Modifier>> item) => Add(item.Key, item.Value);

        public Dice Evaluate(Guid id, string prop)
        {
            var mods = Get(id, prop);

            if (mods != null)
            {
                Dice dice = Dice.Sum(mods.Select(x => x.Evaluate()));
                return dice;
            }

            return "0";
        }

        public int Resolve(Guid id, string prop)
        {
            Dice dice = Evaluate(id, prop);
            return dice.Roll();
        }

        public string[] Describe(Entity entity, string prop)
        {
            var res = new List<string> { $"{entity.Name}.{prop} => {Evaluate(entity.Id, prop)}" };

            var mods = Get(entity.Id, prop);
            var descriptions = mods?
                .SelectMany(x => x.Describe())
                .ToArray();

            res.AddRange(descriptions ?? new string[0]);

            return res.ToArray();
        }

        public void Clear() => _store.Clear();

        public void Clear(Guid id)
        {
            if (!_store.ContainsKey(id))
                return;

            var entityMods = _store[id];
            foreach (var prop in entityMods.Keys)
            {
                var mods = Get(id, prop);
                if (mods?.Any() ?? false)
                {
                    var toRemove = mods.Where(x => x.CanBeCleared()).ToArray();
                    foreach (var remove in toRemove)
                        mods.Remove(remove);

                    if (!mods.Any())
                        entityMods.Remove(prop);

                    if (!entityMods.Any())
                        _store.Remove(id);
                }
            }
        }

        public bool Contains(Modifier mod) => Get(mod.Target.Id, mod.Target.Prop!)?.Any(x => x.Id == mod.Target.Id) ?? false;
        public bool Contains(KeyValuePair<string, List<Modifier>> item) => ContainsKey(item.Key);

        public bool ContainsKey(string key)
        {
            if (!TryParseKey(key, out var res))
                return false;

            return _store.ContainsKey(res.Item1) && _store[res.Item1].ContainsKey(res.Item2);
        }

        public void CopyTo(KeyValuePair<string, List<Modifier>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, List<Modifier>>> GetEnumerator()
        {
            foreach (var id in _store.Keys)
                foreach (var prop in _store[id].Keys)
                    yield return new KeyValuePair<string, List<Modifier>>(MakeKey(id, prop), _store[id][prop]);
        }

        public bool Remove(Modifier mod)
        {
            var res = false;

            if (_store.ContainsKey(mod.Target.Id))
            {
                var entityMods = _store[mod.Target.Id];
                if (!string.IsNullOrEmpty(mod.Target.Prop) && entityMods.ContainsKey(mod.Target.Prop))
                {
                    var mods = entityMods[mod.Target.Prop];
                    var toRemove = mods.FirstOrDefault(x => x.Id == mod.Id && mod.CanBeCleared());

                    if (toRemove != null)
                    {
                        mods.Remove(toRemove);
                        res = true;
                    }

                    if (!mods.Any())
                        entityMods.Remove(mod.Target.Prop);

                    if (!entityMods.Any())
                        _store.Remove(mod.Target.Id);
                }
            }

            return res;
        }

        public bool Remove(Guid id, string prop)
        {
            var res = false;

            if (!_store.ContainsKey(id))
                return false;

            var entityMods = _store[id];

            if (entityMods != null)
            {
                res = entityMods.Remove(prop);
                if (!entityMods.Any())
                    _store.Remove(id);
            }

            return res;
        }

        public bool Remove(string key)
        {
            return TryParseKey(key, out var res)
                ? Remove(res.Item1, res.Item2)
                : false;
        }

        public bool Remove(int turn)
        {
            var toRemove = new List<Modifier>();
            foreach (var mod in this.SelectMany(x => x.Value))
            {
                if (mod.ShouldBeRemoved(turn))
                    toRemove.Add(mod);
            }

            foreach (var mod in toRemove)
                Remove(mod);

            return toRemove.Any();
        }

        public bool Remove(KeyValuePair<string, List<Modifier>> item) => Remove(item.Key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out List<Modifier> value)
        {
            if (TryParseKey(key, out (Guid, string) res) && _store.ContainsKey(res.Item1))
            {
                var entityMods = _store[res.Item1] ?? new Dictionary<string, List<Modifier>>();
                if (entityMods.ContainsKey(res.Item2))
                {
                    value = entityMods[res.Item2];
                    return true;
                }
            }

            value = new List<Modifier>();
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

        private ICollection<List<Modifier>> AllValues()
        {
            var res = new List<List<Modifier>>();
            foreach (var id in _store.Keys)
            {
                foreach (var prop in _store[id].Keys)
                    res.Add(_store[id][prop]);
            }
            return res.ToArray();
        }
    }
}
