using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.Sys
{
    public class EntityStore : IDictionary<Guid, ModdableObject>
    {
        private Graph? _graph;

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

        public void Initialize(Graph graph)
        {
            _graph = graph;
        }

        public void Add(KeyValuePair<Guid, ModdableObject> item) 
            => Add(item.Value);

        public void Add(Guid key, ModdableObject value) 
            => Add(value);

        public void Add(ModdableObject entity, bool setMods = true) => AddRange(new[] { entity }, setMods);

        public void AddRange(IEnumerable<ModdableObject> entities, bool setMods = true)
        {
            var moddableObjects = new List<ModdableObject>();
            foreach (var entity in entities)
            {
                var modObjs = GetModdableObjects(entity);
                moddableObjects.AddRange(modObjs);
            }

            foreach (var moddableObject in moddableObjects)
            {
                //moddableObject.Initialize(_graph!);
                if (_store.ContainsKey(moddableObject.Id))
                    Remove(moddableObject.Id);

                _store.Add(moddableObject.Id, moddableObject);
            }

            if (setMods && _graph != null)
                _graph.Mods.Add(moddableObjects);
        }

        public ModdableObject? Get(Guid? id)
        {
            return id != null && TryGetValue(id.Value, out var entity)
                ? entity
                : null;
        }

        public ModdableObject? Get(PropRef? moddableProperty)
        {
            var id = moddableProperty?.Id;
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

        public IEnumerator<KeyValuePair<Guid, ModdableObject>> GetEnumerator() => _store.GetEnumerator();

        public bool Remove(KeyValuePair<Guid, ModdableObject> item) => _store.Remove(item.Key);

        public bool Remove(Guid key)
        {
            var entity = Get(key);
            if (entity != null)
            {
                var toRemove = GetModdableObjects(entity);
                foreach (var item in toRemove)
                {
                    _store.Remove(item.Id);
                    _graph!.Mods.Remove(item.Id);
                }

                return true;
            }

            return false;
        }

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out ModdableObject value) => _store.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        private List<ModdableObject> GetModdableObjects(object obj)
        {
            var res = new List<ModdableObject>();
            var entity = obj as ModdableObject;
            if (entity != null)
                res.Add(entity);

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                foreach (var item in items)
                {
                    var childEntities = GetModdableObjects(item);
                    res.AddRange(childEntities);
                }
            }

            return res;
        }
    }
}
