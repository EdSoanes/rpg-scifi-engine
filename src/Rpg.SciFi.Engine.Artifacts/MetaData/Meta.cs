using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public abstract class Meta
    {
        protected static object _lock = new object();
        public static List<Entity>? Entities { get; set; }
        public static MetaModifierStore Mods { get; set; } = new MetaModifierStore();

        public static Entity? Get(Guid? id)
        {
            if (id == null) return null;

            lock (_lock)
                return Entities?.SingleOrDefault(x => x.Id == id);
        }

        public static void Add(Entity entity)
        {
            if (entity != null && Get(entity.Id) == null)
            {
                var entities = new List<Entity>();
                InitEntity(entity, "{}", entities.Add);
                InsertEntity(entity);
                SetupEntity(entity);
            }
        }

        public static void Remove(Guid id)
        {
            var toRemove = Get(id);
            if (toRemove != null)
            {
                lock (_lock)
                {
                    Entities?.Remove(toRemove);
                }
            }
        }

        public static Entity? GetByPath(string path)
        {
            if (!path.StartsWith("{}"))
                path = string.IsNullOrEmpty(path)
                    ? "{}"
                    : "{}." + path;

            lock (_lock)
                return Entities?.SingleOrDefault(x => x.MetaData.Path == path);
        }

        public static T? ValueByPath<T>(string path)
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

        protected static void InitEntity(object context, string basePath, Action<Entity>? processContext = null)
        {
            var entity = context as Entity;
            if (entity != null)
            {
                entity.MetaData.Path = basePath;
                processContext?.Invoke(entity);
            }

            foreach (var propertyInfo in context.MetaProperties())
            {
                var items = context.PropertyObjects(propertyInfo, out var isEnumerable);
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? $"[{entity?.Id}]" : "")}";

                foreach (var item in items)
                {
                    InitEntity(item, path, processContext);
                }
            }
        }

        protected static void SetupEntities(List<Entity> entities)
        {
            foreach (var entity in Entities)
                SetupEntity(entity);
        }

        protected static void SetupEntity(Entity entity)
        {
            foreach (var setup in entity.MetaData.SetupMethods)
            {
                var mods = entity.ExecuteFunction<Modifier[]>(setup);
                if (mods != null)
                    Mods.Add(mods);
            }
        }

        protected static void InsertEntities(IEnumerable<Entity> entities)
        {
            lock (_lock)
            {
                if (Entities == null)
                {
                    Entities = entities
                        .OrderBy(x => x.MetaData.Path)
                        .ToList();
                }
                else
                {
                    foreach (var entity in Entities)
                    {
                        if (Get(entity.Id) == null)
                            Entities.Add(entity);
                    }
                }
            }
        }

        protected static void InsertEntity(Entity entity) => InsertEntities(new[] { entity });
    }

    public class Meta<T> : Meta 
        where T : Entity
    {
        public T? Context { get; private set; }

        public void Initialize(T context)
        {
            Mods.Clear();
            Context = context;
            var entities = new List<Entity>();
            InitEntity(Context, "{}", entities.Add);
            InsertEntities(entities);
            SetupEntities(entities);
        }

        public TurnAction? Apply(Character actor, TurnAction turnAction, int diceRoll = 0)
        {
            var modifiers = turnAction.Resolve(diceRoll);

            var actionCost = turnAction.Action;
            if (actionCost != 0)
                Mods.Add(actor.Mod(nameof(TurnPoints.Action), actionCost, (x) => x.Turns.Action, () => Rules.Minus).IsAdditive());

            var exertionCost = turnAction.Exertion;
            if (exertionCost != 0)
                Mods.Add(actor.Mod(nameof(TurnPoints.Exertion), exertionCost, (x) => x.Turns.Exertion, () => Rules.Minus).IsAdditive());

            var focusCost = turnAction.Focus;
            if (focusCost != 0)
                Mods.Add(actor.Mod(nameof(TurnPoints.Focus), focusCost, (x) => x.Turns.Focus, () => Rules.Minus).IsAdditive());

            return turnAction.NextAction();
        }

        public string[] Describe()
        {
            lock (_lock)
            {
                var res = Entities!.OrderBy(x => x.MetaData.Path).Select(x => x.ToString()).ToArray();
                return res;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Context = null;
                Entities = null;
            }
        }

        public string Serialize()
        {
            var json = JsonConvert.SerializeObject(Context, Formatting.None);
            return json;
        }

        public void Deserialize(string state)
        {
            var context = JsonConvert.DeserializeObject<T>(state);
            Initialize(context!);
        }
    }
}
