using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class EntityStore : IDictionary<Guid, ModdableObject>
    {
        private ModStore? _modStore;
        private PropEvaluator? _propEvaluator;
        private TurnManager? _turnManager;

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

        public void Initialize(ModStore modStore, PropEvaluator propEvaluator, TurnManager turnManager)
        {
            _modStore = modStore;
            _propEvaluator = propEvaluator;
            _turnManager = turnManager;

            foreach (var entity in _store.Values)
                entity?.Initialize(modStore, propEvaluator);
        }

        public void Add(ModdableObject entity) => Add(entity.Id, entity);

        public void Add(KeyValuePair<Guid, ModdableObject> item) => Add(item.Key, item.Value);

        public void AddRange(IEnumerable<ModdableObject> entities)
        {
            foreach (var entity in entities)
                Add(entity.Id, entity);
        }

        public void Add(Guid key, ModdableObject value) => Add(value, "{}");

        private void Add(object obj, string basePath)
        {
            var entity = obj as ModdableObject;
            if (entity != null)
            {
                entity.MetaData.Path = basePath;

                if (!_store.ContainsKey(entity.Id))
                    _store.Add(entity.Id, entity);
                else
                    _store[entity.Id] = entity;
            }

            foreach (var propertyInfo in obj.MetaProperties())
            {
                var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? $"[{entity?.Id}]" : "")}";

                foreach (var item in items)
                    Add(item, path);
            }

            SetupMods(entity);
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

        public ModdableObject? GetByPath(string path)
        {
            if (!path.StartsWith("{}"))
                path = string.IsNullOrEmpty(path)
                    ? "{}"
                    : "{}." + path;

            return _store.Values
                ?.SingleOrDefault(x => x.MetaData.Path == path);
        }

        public T? ValueByPath<T>(string path)
            where T : ModdableObject
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
            var toRemove = Get(key);
            if (toRemove != null)
            {
                _store.Remove(key);
                _modStore!.Remove(toRemove.Id);
                return true;
            }

            return false;
        }

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out ModdableObject value) => _store.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        public void Setup() => Setup(Values);
        private void Setup(IEnumerable<ModdableObject> entities)
        {
            //Execute in reverse order to set up child entities first so
            // parent entity mods on children can override child entity mods
            foreach (var entity in entities.Reverse())
                SetupMods(entity);
        }

        private void SetupMods(ModdableObject? entity)
        {
            if (entity != null && _modStore != null && _propEvaluator != null && _turnManager != null)
            {
                entity.Initialize(_modStore, _propEvaluator);

                var mods = entity.Setup();
                if (mods != null)
                    _modStore.Add(mods);
            }
        }
    }
}
