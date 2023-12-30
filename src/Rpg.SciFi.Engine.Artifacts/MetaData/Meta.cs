using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{

    public interface IContext
    {
        Entity? Root { get; }
        void Add(Entity entity);
        void AddRange(IEnumerable<Entity> entities);
        Entity? Get(Guid id);
        Entity? Get(PropReference? moddableProperty);
        bool Remove(Entity entity);
        void AddMod(Modifier mod);
        void AddMods(params Modifier[] mods);

        //List<Modifier>? GetMods(Guid id, string prop);
        //List<Modifier>? GetMods(ModdableProperty? moddableProperty);
        //List<Modifier>? GetMods(Entity entity, string prop);

        MetaModdableProperty? GetModProp(PropReference? moddableProperty);
        MetaModdableProperty? GetModProp(Guid id, string prop);
        MetaModdableProperty? GetModProp<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression) where TEntity : Entity;
        List<Modifier>? GetMods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression) where TEntity: Entity;

        void ClearMods(Guid entityId);

        bool RemoveMods(int currentTurn);
        bool RemoveMods(Guid entityId, string prop);
        bool RemoveMods(Entity entity, string prop);
        bool RemoveMods<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression);

        Dice Evaluate(Guid entityId, string prop);
        Dice Evaluate<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression);

        int Resolve(Guid entityId, string prop);
        int Resolve<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression);

        string[] Describe(Entity entity, string prop, bool includeEntityInformation = false);
        string[] Describe(Modifier mod, bool includeEntityInformation = false);
        string[] Describe(PropReference? moddableProperty, bool includeEntityInformation = false);

        int CurrentTurn { get; }
        void StartEncounter();
        void EndEncounter();
        void IncrementTurn();
        Turns.Action CreateTurnAction(string name, int actionCost, int exertionCost, int focusCost);
    }

    public class Meta<T> : IContext
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
        [JsonProperty] protected MetaEntityStore Entities { get; set; }
        [JsonProperty] protected MetaModifierStore Mods { get; set; }
        [JsonProperty] protected MetaEncounter Encounter { get; set; }

        public Meta()
        {
            Entities = new MetaEntityStore();
            Mods = new MetaModifierStore();
            Encounter = new MetaEncounter();

            InitContext();
        }

        public void Initialize(T context)
        {
            Mods.Clear();
            Entities.Clear();
            Encounter.EndEncounter();

            Context = context;

            Entities.Add(Context);
            Setup();
        }

        public void Add(Entity entity)
        {
            Entities.Add(entity);
            Setup(entity);
        }

        public void AddRange(IEnumerable<Entity> entities)
        {
            Entities.AddRange(entities);
            Setup(entities);
        }

        public Entity? Get(Guid id) => Entities.Get(id);
        public Entity? Get(PropReference? moddableProperty) => Entities.Get(moddableProperty);

        public bool Remove(Entity entity)
        {
            var res = Entities.Remove(entity.Id);
            res |= Mods.Remove(entity.Id);

            return res;
        }

        public void ClearMods(Guid entityId) => Mods.Clear(entityId);

        public void AddMod(Modifier mod) => Mods.Add(mod);
        public void AddMods(params Modifier[] mods) => Mods.Add(mods);

        public MetaModdableProperty? GetModProp(Guid id, string prop) 
            => Mods.Get(id, prop);

        public MetaModdableProperty? GetModProp(PropReference moddableProperty)
            => Mods.Get(moddableProperty);

        public MetaModdableProperty? GetModProp<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : Entity
                => Mods.Get(PropReference.FromPath(entity, expression, true));
        //public List<Modifier>? GetMods(Guid id, string prop) => Mods.Get(id, prop)?.Modifiers;
        //public List<Modifier>? GetMods(ModdableProperty? moddableProperty) => Mods.Get(moddableProperty)?.Modifiers;
        //public List<Modifier>? GetMods(Entity entity, string prop) => Mods.Get(entity.Id, prop)?.Modifiers;
        public List<Modifier>? GetMods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity: Entity
                => Mods.Get(PropReference.FromPath(entity, expression, true))?.Modifiers;

        public bool RemoveMods(int currentTurn) => Mods.Remove(currentTurn);
        public bool RemoveMods(Guid entityId, string prop) => Mods.Remove(entityId, prop);
        public bool RemoveMods(Entity entity, string prop) => Mods.Remove(entity.Id, prop);
        public bool RemoveMods<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression) => Mods.Remove(PropReference.FromPath(entity, expression, true));

        public Dice Evaluate(Guid entityId, string prop) 
            => Mods.Evaluate(entityId, prop);

        public Dice Evaluate<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression)
            => Mods.Evaluate(PropReference.FromPath(entity, expression));

        public int Resolve(Guid entityId, string prop) 
            => Mods.Resolve(entityId, prop);

        public int Resolve<TResult>(Entity entity, Expression<Func<Entity, TResult>> expression) 
            => Mods.Resolve(PropReference.FromPath(entity, expression));

        public string[] Describe(Entity entity, string prop, bool includeEntityInformation = false) => Mods.Describe(entity, prop, includeEntityInformation);
        public string[] Describe(Modifier mod, bool includeEntityInformation = false) => Mods.Describe(mod, includeEntityInformation);
        public string[] Describe(PropReference? moddableProperty, bool includeEntityInformation = false) => Mods.Describe(moddableProperty, includeEntityInformation);

        private void Setup() => Setup(Entities.Values);

        private void Setup(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
                Setup(entity);
        }

        private void Setup(Entity entity)
        {
            foreach (var setup in entity.MetaData.SetupMethods)
            {
                var mods = entity.ExecuteFunction<Modifier[]>(setup);
                if (mods != null)
                    Mods.Add(mods);
            }
        }

        public int CurrentTurn => Encounter.CurrentTurn;
        public void StartEncounter()
        {
            Encounter.StartEncounter();
            Mods.Remove(Encounter.CurrentTurn);
        }

        public void EndEncounter()
        {
            Encounter.EndEncounter();
            Mods.Remove(Encounter.CurrentTurn);
        }

        public void IncrementTurn()
        {
            Encounter.IncrementTurn();
            Mods.Remove(Encounter.CurrentTurn);
        }

        public Turns.Action CreateTurnAction(string name, int actionCost, int exertionCost, int focusCost)
        {
            var action = new Turns.Action(name, actionCost, exertionCost, focusCost);
            Entities.Add(action);
            Setup(action);

            return action;
        }

        public Turns.Action? Apply(Character actor, Turns.Action turnAction, int diceRoll = 0)
        {
            var actionCost = turnAction.ActionCost;
            if (actionCost != 0)
                Mods.Add(CostModifier.Create(actionCost, actor, x => x.Turns.Action, () => Rules.Minus));

            var exertionCost = turnAction.ExertionCost;
            if (exertionCost != 0)
                Mods.Add(CostModifier.Create(exertionCost, actor, x => x.Turns.Exertion, () => Rules.Minus));

            var focusCost = turnAction.FocusCost;
            if (focusCost != 0)
                Mods.Add(CostModifier.Create(focusCost, actor, x => x.Turns.Focus, () => Rules.Minus));

            var modifiers = turnAction.Resolve(diceRoll);
            Mods.Add(modifiers);

            return turnAction.NextAction();
        }

        public string[] Describe() => Entities.Values.OrderBy(x => x.MetaData.Path).Select(x => x.ToString()).ToArray();

        public void Clear()
        {
            Context = null;
            Entities = null;
        }

        public string Serialize()
        {
            var json = JsonConvert.SerializeObject(this, JsonSettings);
            return json;
        }

        public static Meta<T>? Deserialize(string json)
        {
            var meta = JsonConvert.DeserializeObject<Meta<T>>(json, JsonSettings);
            meta?.InitContext();

            return meta;
        }

        private void InitContext()
        {
            Entities?.PropertyValue("Context", this as IContext);
            if (Entities?.Values.Any() ?? false)
            {
                foreach (var entity in Entities.Values)
                    entity.PropertyValue("Context", this as IContext);
            }

            Context = Entities?.Get(Context?.Id) as T;
            Mods?.PropertyValue("Context", this as IContext);
            //Mods?.EvaluateAll();
        }
    }
}
