using Rpg.Sys.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Rpg.Sys
{
    public class ModStore : IDictionary<string, ModProp>
    {
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

        public int Count => _store.Sum(x => x.Value.Sum(y => y.Value.AllModifiers.Count()));

        public bool IsReadOnly => false;

        public Modifier[]? GetMods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => Get(PropRef.FromPath(entity, expression, true))?.AllModifiers;

        public ModProp? Get<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => Get(PropRef.FromPath(entity, expression, true));

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

        public ModProp[] Get(Guid entityId)
        {
            return _store.ContainsKey(entityId)
                ? _store[entityId].Values.ToArray()
                : new ModProp[0];
        }

        public ModProp? Get(Guid? entityId, string? prop)
        {
            if (entityId == null  || string.IsNullOrEmpty(prop))
                return null;

            return TryGetValue(MakeKey(entityId.Value, prop), out var res)
                ? res
                : null;
        }

        public void Add(params Modifier[] mods)
        {
            foreach (var mod in mods)
            {
                var modProp = Get(mod.Target.Id, mod.Target.Prop);
                if (modProp != null)
                    modProp.Add(mod);
            }
        }

        public void Add(params ModProp[] modProps)
        {
            foreach (var modProp in modProps)
                _Add(modProp);
        }

        public void Add(string key, ModProp value) => _Add(value);
        public void Add(KeyValuePair<string, ModProp> item) => _Add(item.Value);

        private void _Add(ModProp value)
        {
            if (!_store.ContainsKey(value.EntityId))
                _store.Add(value.EntityId, new Dictionary<string, ModProp>());
            else
                _store[value.EntityId].Remove(MakeKey(value.EntityId, value.Prop));

            _store[value.EntityId].Add(value.Prop, value);
        }

        public void Clear() 
            => _store.Clear();

        public bool Contains(KeyValuePair<string, ModProp> item) 
            => ContainsKey(item.Key);

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

        private bool Remove(Guid? entityId, string? prop)
        {
            bool res = false;

            var modProp = Get(entityId, prop);
            if (modProp != null)
                res = modProp.Remove().Any();

            return res;
        }

        public bool Remove(params ModProp[] modProps)
        {
            foreach (var key in modProps.Select(x => MakeKey(x.EntityId, x.Prop)))
                Remove(key);

            return modProps.Any();
        }

        public bool Remove(string key)
        {
            return TryParseKey(key, out var res)
                ? Remove(res.Item1, res.Item2)
                : false;
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

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();

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

        public List<ModProp> GetAffectedModProps(ModProp modProp)
        {
            var res = new List<ModProp>
            {
                modProp
            };

            res.AddRange(GetAffectedModProps(modProp.EntityId, modProp.Prop));

            return res;
        }

        public List<ModProp> GetAffectedModProps(Guid id, string prop)
        {
            var res = new List<ModProp>();

            var modProps = AllValues().Where(x => x.AffectedBy(id, prop));
            foreach (var mp in modProps)
                res.AddRange(GetAffectedModProps(mp));

            return res;
        }

        public T? Iterate<T>(Modifier mod, Func<ModProp, T> op)
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
