using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public abstract class Meta
    {
        protected static object _lock = new object();
        protected static List<Entity>? Entities { get; set; }

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
                InitializeEntityGraph(entity, "{}");
                lock (_lock)
                    Entities?.Add(entity);
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

        //public static TurnAction CreateAction(string name, int actionCost, int exertionCost, int focusCost)
        //{
        //    var action = new TurnAction(name, actionCost, exertionCost, focusCost);
        //    Entities.Add(action);

        //    return action;
        //}

        public static Entity? GetByPath(string path)
        {
            if (!path.StartsWith("{}"))
                path = string.IsNullOrEmpty(path)
                    ? "{}"
                    : "{}." + path;

            lock (_lock)
                return Entities?.SingleOrDefault(x => x.Meta.Path == path);
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

        protected static void InitializeEntityGraph(object context, string basePath, Action<Entity>? processContext = null)
        {
            var entity = context as Entity;
            if (entity != null)
            {
                entity.Meta.Path = basePath;
                processContext?.Invoke(entity);
            }

            foreach (var propertyInfo in context.MetaProperties())
            {
                var items = context.PropertyObjects(propertyInfo, out var isEnumerable);
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? $"[{entity?.Id}]" : "")}";

                foreach (var item in items)
                {
                    InitializeEntityGraph(item, path, processContext);
                }
            }
        }
    }

    public class Meta<T> : Meta 
        where T : Entity
    {
        public T? Context { get; private set; }

        public void Initialize(T context)
        {
            Context = context;
            var entities = new List<Entity>();
            InitializeEntityGraph(Context, "{}", entities.Add);

            lock (_lock )
            {
                Entities = entities
                    .OrderBy(x => x.Meta.Path)
                    .ToList();

                foreach (var entity in Entities)
                {
                    foreach (var setup in entity.Meta.SetupMethods)
                        entity.ExecuteAction(setup);
                }
            }
        }

        public TurnAction? Apply(Character actor, TurnAction turnAction, int diceRoll = 0)
        {
            var modifiers = turnAction.Resolve(diceRoll);

            var actionCost = turnAction.Costs.Action;
            if (actionCost != 0)
                actor.Mod(nameof(TurnPoints.Action), actionCost, (x) => x.Turns.Action, () => Rules.Minus).IsInstant().Apply();

            var exertionCost = turnAction.Costs.Exertion;
            if (exertionCost != 0)
                actor.Mod(nameof(TurnPoints.Action), exertionCost, (x) => x.Turns.Exertion, () => Rules.Minus).IsInstant().Apply();

            var focusCost = turnAction.Costs.Focus;
            if (focusCost != 0)
                actor.Mod(nameof(TurnPoints.Action), focusCost, (x) => x.Turns.Focus, () => Rules.Minus).IsInstant().Apply();

            foreach (var modifier in modifiers)
            {
                var res = modifier.Resolve();
                modifier.Apply(); // Will need to be able to roll dice for modifiers as required.
            }

            return turnAction.NextAction();
        }

        public string[] Describe()
        {
            lock (_lock)
            {
                var res = Entities!.OrderBy(x => x.Meta.Path).Select(x => x.ToString()).ToArray();
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
