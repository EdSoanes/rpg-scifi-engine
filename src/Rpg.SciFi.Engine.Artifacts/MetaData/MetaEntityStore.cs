using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaEntityStore : IDictionary<Guid, Entity>
    {
        private readonly Dictionary<Guid, Entity> _store = new Dictionary<Guid, Entity>();
        protected IContext Context { get; set; }

        public Entity this[Guid key]
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

        public ICollection<Entity> Values => _store.Values;

        public int Count => _store.Count;

        public bool IsReadOnly => false;

        public void Add(Entity entity) => Add(entity.Id, entity);

        public void Add(KeyValuePair<Guid, Entity> item) => Add(item.Key, item.Value);

        public void AddRange(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
                Add(entity.Id, entity);
        }

        public void Add(Guid key, Entity value)
        {
            Init(value, "{}", entity =>
            {
                if (!_store.ContainsKey(entity.Id))
                    _store.Add(entity.Id, entity);
                else
                    _store[entity.Id] = entity;
            });
        }

        public Entity? Get(Guid? id)
        {
            return id != null && TryGetValue(id.Value, out var entity)
                ? entity
                : null;
        }

        public Entity? Get(ModdableProperty? moddableProperty)
        {
            return moddableProperty?.Id != null && TryGetValue(moddableProperty.Id, out var entity)
                ? entity
                : null;
        }

        public Entity? GetByPath(string path)
        {
            if (!path.StartsWith("{}"))
                path = string.IsNullOrEmpty(path)
                    ? "{}"
                    : "{}." + path;

            return _store.Values
                ?.SingleOrDefault(x => x.MetaData.Path == path);
        }

        public T? ValueByPath<T>(string path)
            where T : Entity
        {
            var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            path = parts.Length == 1
                ? string.Empty
                : string.Join('.', parts.Take(parts.Length - 1));

            var prop = parts.Last();

            var entity = GetByPath(path);
            if (entity != null)
                return entity.PropertyValue<T>(prop);

            return default;
        }

        public void Clear() => _store.Clear();

        public bool Contains(KeyValuePair<Guid, Entity> item) => _store.Contains(item);

        public bool ContainsKey(Guid key) => _store.ContainsKey(key);

        public void CopyTo(KeyValuePair<Guid, Entity>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<Guid, Entity>> GetEnumerator() => _store.GetEnumerator();

        public bool Remove(KeyValuePair<Guid, Entity> item) => _store.Remove(item.Key);

        public bool Remove(Guid key)
        {
            var toRemove = Get(key);
            return toRemove != null
                ? _store.Remove(key)
                : false;
        }

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out Entity value) => _store.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        protected void Init(object obj, string basePath, Action<Entity>? processContext = null)
        {
            var entity = obj as Entity;
            if (entity != null)
            {
                entity.MetaData.Path = basePath;
                entity.PropertyValue("Context", Context);
                processContext?.Invoke(entity);
            }

            foreach (var propertyInfo in obj.MetaProperties())
            {
                var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? $"[{entity?.Id}]" : "")}";

                foreach (var item in items)
                    Init(item, path, processContext);
            }
        }
    }
}
