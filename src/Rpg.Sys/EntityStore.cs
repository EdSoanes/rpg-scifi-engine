using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.Sys
{
    public class EntityStore : IDictionary<Guid, ModdableObject>
    {
        private readonly Dictionary<Guid, ModdableObject> _store = new Dictionary<Guid, ModdableObject>();

        public ModdableObject this[Guid key]
        {
            get
            {
                return _store[key];
            }
            set
            {
                if (value != null)
                {
                    if (_store.ContainsKey(key))
                        _store[key] = value;
                    else
                        _store.Add(key, value);
                }
            }
        }

        public ICollection<Guid> Keys => _store.Keys;

        public ICollection<ModdableObject> Values => _store.Values;

        public int Count => _store.Count;

        public bool IsReadOnly => false;

        public void Add(KeyValuePair<Guid, ModdableObject> item) 
            => Add(item.Value);

        public void Add(Guid key, ModdableObject value) 
            => Add(value);

        public void Add(ModdableObject entity) 
            => Add(new[] { entity });

        public void Add(params ModdableObject[] entities)
        {
            foreach (var entity in entities)
                _store.Add(entity.Id, entity);
        }

        public ModdableObject? Get(Guid? id)
        {
            return id != null && TryGetValue(id.Value, out var entity)
                ? entity
                : null;
        }

        public ModdableObject? Get(PropRef? moddableProperty)
        {
            var id = moddableProperty?.EntityId;
            return id != null && TryGetValue(id.Value, out var entity)
                ? entity
                : null;
        }

        public void Clear() => _store.Clear();

        public bool Contains(KeyValuePair<Guid, ModdableObject> item) => _store.Contains(item);

        public bool ContainsKey(Guid key) => _store.ContainsKey(key);

        public void CopyTo(KeyValuePair<Guid, ModdableObject>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<Guid, ModdableObject>> GetEnumerator() 
            => _store.GetEnumerator();

        public bool Remove(KeyValuePair<Guid, ModdableObject> item) 
            => _store.Remove(item.Key);

        public bool Remove(Guid key)
            => _store.Remove(key);

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out ModdableObject value) 
            => _store.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() 
            => _store.GetEnumerator();
    }
}
