using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class EntityStore : IDictionary<Guid, Entity>
    {
        private ModStore? _modStore;
        private IPropEvaluator? _propEvaluator;
        private TurnManager? _turnManager;

        private readonly Dictionary<Guid, Entity> _store = new Dictionary<Guid, Entity>();

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

        public void Initialize(ModStore modStore, IPropEvaluator propEvaluator, TurnManager turnManager)
        {
            _modStore = modStore;
            _propEvaluator = propEvaluator;
            _turnManager = turnManager;
        }

        public void Add(Entity entity) => Add(entity.Id, entity);

        public void Add(KeyValuePair<Guid, Entity> item) => Add(item.Key, item.Value);

        public void AddRange(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
                Add(entity.Id, entity);
        }

        public void Add(Guid key, Entity value) => Add(value, "{}");

        private void Add(object obj, string basePath)
        {
            var entity = obj as Entity;
            if (entity != null)
            {
                entity.MetaData.Path = basePath;

                if (!_store.ContainsKey(entity.Id))
                    _store.Add(entity.Id, entity);
                else
                    _store[entity.Id] = entity;

                Setup(entity);
            }

            foreach (var propertyInfo in obj.MetaProperties())
            {
                var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? $"[{entity?.Id}]" : "")}";

                foreach (var item in items)
                    Add(item, path);
            }
        }

        public Entity? Get(Guid? id)
        {
            return id != null && TryGetValue(id.Value, out var entity)
                ? entity
                : null;
        }

        public Entity? Get(PropRef? moddableProperty)
        {
            var id = moddableProperty?.Id;
            return id != null && TryGetValue(id.Value, out var entity)
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
            if (toRemove != null)
            {
                _store.Remove(key);
                _modStore!.Remove(toRemove.Id);
                return true;
            }

            return false;
        }

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out Entity value) => _store.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        public void Setup() => Setup(Values);
        private void Setup(IEnumerable<Entity> entities)
        {
            //Execute in reverse order to set up child entities first so
            // parent entity mods on children can override child entity mods
            foreach (var entity in entities.Reverse())
                Setup(entity);
        }

        private void Setup(Entity entity)
        {
            if (_modStore != null && _propEvaluator != null && _turnManager != null)
            {
                entity.Initialize(_modStore, _propEvaluator, _turnManager);
                foreach (var setup in entity.MetaData.SetupMethods)
                {
                    var mods = entity.ExecuteFunction<Modifier[]>(setup);
                    if (mods != null)
                        _modStore.Add(mods);
                }
            }
        }
    }
}
