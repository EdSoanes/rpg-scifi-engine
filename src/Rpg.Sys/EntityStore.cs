using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Rpg.Sys.Moddable;

namespace Rpg.Sys
{
    public class EntityStore : IDictionary<Guid, ModObject>
    {
        private readonly Dictionary<Guid, ModObject> _store = new Dictionary<Guid, ModObject>();

        public ModObject this[Guid key]
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

        public ICollection<ModObject> Values => _store.Values;

        public int Count => _store.Count;

        public bool IsReadOnly => false;

        public void Add(KeyValuePair<Guid, ModObject> item) 
            => Add(item.Value);

        public void Add(Guid key, ModObject value) 
            => Add(value);

        public void Add(ModObject entity) 
            => Add(new[] { entity });

        public void Add(params ModObject[] entities)
        {
            foreach (var entity in entities)
                _store.Add(entity.Id, entity);
        }

        public ModObject? Get(Guid? id)
        {
            return id != null && TryGetValue(id.Value, out var entity)
                ? entity
                : null;
        }

        public ModObject? Get(PropRef? moddableProperty)
        {
            var id = moddableProperty?.EntityId;
            return id != null && TryGetValue(id.Value, out var entity)
                ? entity
                : null;
        }

        public void Clear() => _store.Clear();

        public bool Contains(KeyValuePair<Guid, ModObject> item) => _store.Contains(item);

        public bool ContainsKey(Guid key) => _store.ContainsKey(key);

        public void CopyTo(KeyValuePair<Guid, ModObject>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<Guid, ModObject>> GetEnumerator() 
            => _store.GetEnumerator();

        public bool Remove(KeyValuePair<Guid, ModObject> item) 
            => _store.Remove(item.Key);

        public bool Remove(Guid key)
            => _store.Remove(key);

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out ModObject value) 
            => _store.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() 
            => _store.GetEnumerator();
    }
}
