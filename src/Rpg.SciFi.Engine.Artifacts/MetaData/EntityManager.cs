using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class EntityManager<T> : IEntityManager
        where T : Entity
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Reuse
        };

        [JsonIgnore] public Entity? Root { get => Context; }
        [JsonProperty] public T? Context { get; private set; }
        [JsonProperty] protected EntityStore EntityStore { get; set; }
        [JsonProperty] protected ModStore ModStore { get; set; }
        [JsonProperty] protected TurnManager Turns { get; set; }

        private PropEvaluator Evaluator { get; set; }

        public EntityManager()
        {
            ModStore = new ModStore();
            EntityStore = new EntityStore();
            Turns = new TurnManager();
            Evaluator = new PropEvaluator();

            InitContext();
        }

        public void Initialize(T context)
        {
            ModStore.Clear();
            EntityStore.Clear();
            Turns.EndEncounter();

            Context = context;

            EntityStore.Add(Context);
            //EntityStore.Setup();
        }

        public void Add(Entity entity) => EntityStore.Add(entity);
        public void AddRange(IEnumerable<Entity> entities) => EntityStore.AddRange(entities);

        public Entity? Get(Guid id) => EntityStore.Get(id);
        public Entity? Get(PropRef? moddableProperty) => EntityStore.Get(moddableProperty);

        public bool Remove(Entity entity)
        {
            var res = EntityStore.Remove(entity.Id);
            res |= ModStore.Remove(entity.Id);

            return res;
        }

        public void ClearMods(Guid entityId) 
            => ModStore.Clear(entityId);

        public void AddMod(Modifier mod) 
            => ModStore.Add(mod);

        public void AddMods(params Modifier[] mods) 
            => ModStore.Add(mods);

        public MetaModdableProperty? GetModProp<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : Entity
                => ModStore.Get(PropRef.FromPath(entity, expression, true));

        public List<Modifier>? GetMods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity: Entity
                => ModStore.Get(PropRef.FromPath(entity, expression, true))?.Modifiers;

        public bool RemoveMods(int currentTurn) 
            => ModStore.Remove(currentTurn);

        public bool RemoveMods(Guid entityId, string prop) 
            => ModStore.Remove(entityId, prop);

        public bool RemoveMods(Entity entity, string prop) 
            => ModStore.Remove(entity.Id, prop);

        public bool RemoveMods<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression) 
            => ModStore.Remove(PropRef.FromPath(entity, expression, true));

        public Dice Evaluate(IEnumerable<Modifier> mods) 
            => Evaluator.Evaluate(mods);

        public Dice Evaluate(Guid entityId, string prop) 
            => Evaluator.Evaluate(entityId, prop);

        public Dice Evaluate<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression)
        {
            var propRef = PropRef.FromPath(entity, expression);
            return Evaluator.Evaluate(propRef.Id!.Value, propRef.Prop);
        }

        public string[] Describe(Guid id, string prop, bool addEntityInfo = false) 
            => Evaluator.Describe(id, prop, addEntityInfo);

        public string[] Describe(Modifier mod, bool addEntityInfo = false) 
            => Evaluator.Describe(mod, addEntityInfo);

        public int CurrentTurn => Turns.Current;
        public void StartEncounter()
        {
            Turns.StartEncounter();
            ModStore.Remove(Turns.Current);
        }

        public void EndEncounter()
        {
            Turns.EndEncounter();
            ModStore.Remove(Turns.Current);
        }

        public void IncrementTurn()
        {
            Turns.Increment();
            ModStore.Remove(Turns.Current);
        }

        public Turns.Action CreateTurnAction(string name, int actionCost, int exertionCost, int focusCost)
            => Turns.CreateAction(name, actionCost, exertionCost, focusCost);

        public Turns.Action? Apply(Character actor, Turns.Action turnAction, int diceRoll = 0)
            => Turns.Apply(actor, turnAction, diceRoll);

        public string[] Describe() => EntityStore.Values.OrderBy(x => x.MetaData.Path).Select(x => x.ToString()).ToArray();

        public string Serialize()
        {
            var json = JsonConvert.SerializeObject(this, JsonSettings);
            return json;
        }

        public static EntityManager<T>? Deserialize(string json)
        {
            var meta = JsonConvert.DeserializeObject<EntityManager<T>>(json, JsonSettings)!;
            meta.InitContext();
            meta.Context = meta.EntityStore.Get(meta.Context!.Id) as T;
            return meta;
        }

        private void InitContext()
        {
            Evaluator.Initialize(ModStore, EntityStore);
            ModStore.Initialize(Evaluator);
            EntityStore.Initialize(ModStore, Evaluator, Turns);
            Turns.Initialize(ModStore, EntityStore);
        }
    }
}
