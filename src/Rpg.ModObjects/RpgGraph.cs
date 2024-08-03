using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public class RpgGraph
    {
        private List<PropRef> UpdatedProps = new List<PropRef>();

        [JsonProperty] public RpgObject Context { get; private set; }
        [JsonProperty] protected Dictionary<string, RpgObject> ObjectStore { get; set; } = new Dictionary<string, RpgObject>();
        [JsonProperty] public ITimeEngine Time { get; private set; }

        public RpgGraph(RpgObject context)
        {
            RpgTypeScan.RegisterAssembly(GetType().Assembly);

            Context = context;

            Time = new TurnBasedTimeEngine();
            Time.OnTimeEvent += OnTimeEvent;
            Time.Begin();
        }

        public RpgGraph(RpgGraphState state)
        {
            RpgTypeScan.RegisterAssembly(GetType().Assembly);

            Context = state.Entities.First(x => x.Id == state.ContextId);
            Time = state.Time!;
            Time.OnTimeEvent += OnTimeEvent;

            foreach (var entity in state.Entities.SelectMany(x => x.Traverse()))
            {
                entity.OnBeforeTime(this, entity);
                ObjectStore.Add(entity.Id, entity);
            }
        }

        public RpgGraphState GetGraphState()
        {
            var state = new RpgGraphState
            {
                ContextId = Context.Id,
                Entities = ObjectStore.Values.Where(x => x is RpgEntity || x is RpgActivity).ToList(),
                Time = Time,
            };

            return state;
        }

        //public string Serialize()
        //{
        //    var json = RpgSerializer.Serialize(GetGraphState());
        //    return json;
        //}

        //public static RpgGraph Deserialize(string stateJson)
        //{
        //    var state = RpgSerializer.Deserialize<RpgGraphState>(stateJson);
        //    return new RpgGraph(state);
        //}

        public T? Locate<T>(string? id)
            where T : class
        {
            if (typeof(T).IsAssignableTo(typeof(RpgObject)))
                return GetObject(id) as T;

            if (typeof(T).IsAssignableTo(typeof(States.State)))
                return GetState(id) as T;

            if (typeof(T).IsAssignableTo(typeof(ModSet)))
                return GetModSet(id) as T;

            if (typeof(T).IsAssignableTo(typeof(Mod)))
                return GetObjects().SelectMany(x => x.GetMods()).FirstOrDefault(x => x.Id == id) as T;

            return null;
        }

        public Mod[] GetActiveMods(RpgObject? rpgObj, string? prop, Func<Mod, bool>? filterFunc = null)
            => rpgObj?.GetActiveMods(prop)
                .Where(x => filterFunc == null || filterFunc(x))
                .ToArray() ?? Array.Empty<Mod>();

        public Mod[] GetActiveMods(PropRef? propRef, Func<Mod, bool>? filterFunc = null)
            => GetActiveMods(GetObject(propRef?.EntityId), propRef?.Prop, filterFunc);

        public Mod[] GetActiveMods(Func<Mod, bool>? filterFunc = null)
            => ObjectStore.Values
                .SelectMany(x => x.GetActiveMods())
                .Where(x => filterFunc == null || filterFunc(x))
                .ToArray();

        public Mod[] GetMods(PropRef? propRef, Func<Mod, bool>? filterFunc = null)
            => GetObject(propRef?.EntityId)
                ?.GetMods(propRef?.Prop)
                .Where(x => filterFunc == null || filterFunc(x))
                .ToArray() ?? Array.Empty<Mod>();

        public void AddMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetObject(modGroup.Key)?.AddMods(modGroup.ToArray());
        }

        public void AddModSets(params ModSet[] modSets)
        {
            foreach (var modSet in modSets)
            {
                var entity = GetObject(modSet.OwnerId)!;
                entity.AddModSet(modSet);
            }
        }

        public void EnableMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetObject(modGroup.Key)?.EnableMods(modGroup.ToArray());
        }

        public void DisableMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetObject(modGroup.Key)?.DisableMods(modGroup.ToArray());
        }

        public void ApplyMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetObject(modGroup.Key)?.ApplyMods(modGroup.ToArray());
        }

        public void UnapplyMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetObject(modGroup.Key)?.UnapplyMods(modGroup.ToArray());
        }

        public void RemoveMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetObject(modGroup.Key)?.RemoveMods(modGroup.ToArray());
        }

        public List<PropRef> GetPropsAffectedBy(PropRef propRef)
        {
            var res = new List<PropRef>();
            res.Merge(propRef);

            var propsAffectedBy = new List<PropRef>();
            foreach (var entity in GetObjects())
            {
                var affectedBy = entity.GetProps()
                    .Where(x => x.IsAffectedBy(propRef))
                    .Select(x => new PropRef(entity.Id, x.Prop))
                    .Distinct();

                res.Merge(affectedBy);

                foreach (var propAffectedBy in affectedBy)
                {
                    var parentAffectedBy = GetPropsAffectedBy(propAffectedBy);
                    res.Merge(parentAffectedBy);
                }
            }

            return res;
        }

        public bool AddEntity(RpgObject entity)
        {
            if (!ObjectStore.ContainsKey(entity.Id))
            {
                ObjectStore.Add(entity.Id, entity);
                if (Time.Current != TimePoints.BeforeTime)
                {
                    entity.OnBeforeTime(this);
                    entity.OnBeginningOfTime(this);
                    entity.OnStartLifecycle(this, Time.Current);
                }

                return true;
            }

            return false;
        }

        public bool RemoveEntity(RpgObject entity)
        {
            if (ObjectStore.ContainsKey(entity.Id))
            {
                ObjectStore.Remove(entity.Id);
                return true;
            }

            return false;
        }

        //public T? GetEntity<T>(string? objectId)
        //    where T : RpgEntity
        //{
        //            => objectId != null && ObjectStore.ContainsKey(objectId)
        //    ? ObjectStore[objectId] as T
        //    : null;
        //}

        public T? GetObject<T>(string? objectId)
            where T : RpgObject
                => objectId != null && ObjectStore.ContainsKey(objectId)
                    ? ObjectStore[objectId] as T
                    : null;

        public RpgObject? GetObject(string? objectId)
            => objectId != null && ObjectStore.ContainsKey(objectId)
                ? ObjectStore[objectId]
                : null;

        public IEnumerable<RpgObject> GetObjects()
            => ObjectStore.Values;

        public IEnumerable<T> GetObjects<T>()
            where T : RpgObject
                => ObjectStore.Values
                    .Where(x => x is T)
                    .Cast<T>();      

        public IEnumerable<RpgObject> GetScopedEntities(string rpgObjId, ModScope scope)
        {
            var entity = GetObject(rpgObjId);
            if (entity is RpgComponent)
                entity = GetObject((entity as RpgComponent)!.EntityPropRef!.EntityId);

            var all = ObjectStore.Values.Where(x => x.Id == entity!.Id || (x as RpgComponent)?.EntityPropRef?.EntityId == entity!.Id);
            if (scope == ModScope.Objects)
                return all.Where(x => x.Id != rpgObjId);

            if (scope == ModScope.Components)
            {
                var components = all
                    .Where(x => x is RpgComponent && x.Id != rpgObjId)
                    .Cast<RpgComponent>();

                return components;
            }

            return all.Where(x => x.Id == rpgObjId);
        }
            

        public ModSet[] GetModSets()
            => ObjectStore.Values
                .SelectMany(x => x.ModSets.Values)
                .ToArray();

        public ModSet[] GetModSets(RpgObject rpgObj, Func<ModSet, bool> filterFunc)
            => rpgObj.ModSets.Values
                .Where(x => filterFunc(x))
                .ToArray();

        public ModSetBase? GetModSet(string? modSetId)
        {
            if (modSetId == null) 
                return null;

            foreach (var rpgObj in ObjectStore.Values)
            {
                var modSet = rpgObj.GetModSet(modSetId);
                if (modSet != null)
                    return modSet;
            }

            return null;
        }

        public States.State? GetState(string? stateId)
        {
            if (stateId == null)
                return null;

            foreach (var rpgObj in ObjectStore.Values)
            {
                if (rpgObj is RpgEntity entity)
                {
                    var state = entity.GetStateById(stateId);
                    if (state != null)
                        return state;
                }

            }

            return null;
        }

        public void RemoveModSet(string modSetId)
        {
            foreach (var rpgObj in ObjectStore.Values)
            {
                var modSet = rpgObj.GetModSet(modSetId);
                if (modSet != null)
                {
                    rpgObj.RemoveModSet(modSetId);
                    return;
                }
            }
        }

        public void RemoveModSet(RpgObject rpgObj, string modSetId)
        {
            var modSet = rpgObj.GetModSet(modSetId);
            if (modSet != null)
                rpgObj.RemoveModSet(modSet.Id);
        }

        public void RemoveModSetByName(RpgObject rpgObj, string name)
        {
            var modSet = rpgObj.GetModSet(name);
            if (modSet != null)
                rpgObj.RemoveModSet(modSet.Id);
        }

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        public Dice? CalculatePropValue(PropRef propRef, Func<Mod, bool>? filterFunc = null)
        {
            var entity = GetObject(propRef.EntityId);
            return CalculatePropValue(entity, propRef.Prop, filterFunc);
        }

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        public Dice? CalculatePropValue(RpgObject? entity, string? prop, Func<Mod, bool>? filterFunc = null)
        {
            if (entity == null || string.IsNullOrEmpty(prop))
                return null;

            var mods = filterFunc != null
                ? GetActiveMods(entity, prop, filterFunc)
                : GetActiveMods(entity, prop);

            var dice = CalculateModsValue(mods);
            return dice;
        }

        public Dice? CalculateModsValue(Mod[] mods)
        {
            var selectedMods = mods.Where(x => !(x.Behavior is Threshold));
            if (!selectedMods.Any())
                return null;

            Dice? dice = null;
            foreach (var mod in selectedMods)
            {
                var val = CalculateModValue(mod);
                if (val != null)
                    dice = dice != null ? dice.Value + val.Value : val;
            }

            return dice != null ? ApplyThreshold(mods, dice!.Value) : null;
        }

        public Dice? CalculateInitialPropValue(RpgObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseInitMod);

        public Dice? CalculateBasePropValue(RpgObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseMod || mod.IsBaseInitMod);

        public Dice? CalculateModValue(Mod? mod)
        {
            if (mod == null)
                return null;

            Dice? value = mod.SourceValue ?? GetPropValue(GetObject(mod.SourcePropRef!.EntityId), mod.SourcePropRef.Prop);

            if (value != null && mod.SourceValueFunc != null)
            {
                var args = new Dictionary<string, object?>();
                args.Add(mod.SourceValueFunc.Args.First().Name, value);

                var entity = GetObject(mod.SourceValueFunc.EntityId);
                value = entity != null
                    ? mod.SourceValueFunc.Execute(entity, args)
                    : mod.SourceValueFunc.Execute(args);
            }

            return value;
        }

        private Dice ApplyThreshold(Mod[] mods, Dice dice)
        {
            var threshold = mods.FirstOrDefault(x => x.Behavior is Threshold)?.Behavior as Threshold;
            if (threshold != null && dice.IsConstant)
            {
                if (dice.Roll() < threshold.Min)
                    dice = threshold.Min;
                else if (dice.Roll() > threshold.Max)
                    dice = threshold.Max;
            }

            return dice;
        }

        public Dice? GetPropValue(RpgObject? entity, string? prop)
        {
            if (entity == null || string.IsNullOrEmpty(prop))
                return null;

            var val = entity.PropertyValue(prop, out var propExists);
            if (propExists && val != null)
            {
                if (val is Dice)
                    return (Dice)val;
                else if (val is int)
                    return (int)val;
            }

            if (!propExists)
            {
                var mods = GetActiveMods(entity, prop);
                var dice = CalculateModsValue(mods);

                return dice;
            }

            return null;
        }

        public void OnPropUpdated(PropRef propRef)
            => UpdatedProps.Merge(propRef);

        public void OnPropsUpdated()
        {
            foreach (var obj in ObjectStore.Values)
                foreach (var modProp in obj.GetProps())
                    UpdatedProps.Merge(modProp);
        }

        public void SetPropValue(PropRef propRef)
        {
            var entity = GetObject(propRef.EntityId);
            SetPropValue(entity, propRef.Prop);
        }

        public void SetPropValue(RpgObject? entity, string prop)
        {
            var oldValue = GetPropValue(entity, prop);
            var newValue = CalculatePropValue(entity, prop);

            if (oldValue == null || oldValue != newValue)
                entity.PropertyValue(prop, newValue);
        }

        private void UpdateProps()
        {
            //It is IMPORTANT that Merge() adds the propRefs in the right order because
            // after this step the assumption is that they can be updated from top to bottom
            // so that all child props are updated before their parents
            var propsToUpdate = new List<PropRef>();
            foreach (var propRef in UpdatedProps)
            {
                var affectedBy = GetPropsAffectedBy(propRef);
                propsToUpdate.Merge(affectedBy);
            }

            //Clear the UpdatedProps queue.
            UpdatedProps.Clear();

            foreach (var propRef in propsToUpdate)
                SetPropValue(propRef);
        }

        private void OnTimeEvent(object? obj, NotifyTimeEventEventArgs args)
        {
            switch (args.Time.Type)
            {
                case nameof(TimePoints.BeforeTime):
                    OnBeforeTime();
                    break;
                case nameof(TimePoints.BeginningOfTime): 
                    OnBeginningOfTime(); 
                    break;
                default:
                    OnTimeUpdates();
                    break;
            }
        }

        private void OnBeforeTime()
        {
            ObjectStore.Clear();

            foreach (var entity in Context.Traverse())
            {
                entity.OnBeforeTime(this, entity);
                AddEntity(entity);
            }
        }

        private void OnBeginningOfTime()
        {
            var created = new List<RpgObject>();
            foreach (var entity in ObjectStore.Values.Where(x => x.Expiry == LifecycleExpiry.Pending))
            {
                entity.OnBeginningOfTime(this, entity);
                created.Add(entity);
            }

            if (created.Any())
            {
                OnPropsUpdated();
                UpdateProps();
            
                foreach (var entity in created)
                    entity.OnStartLifecycle(this, Time.Current);
            }

            OnPropsUpdated();
            UpdateProps();
        }

        private void OnTimeUpdates()
        {
            foreach (var entity in ObjectStore.Values)
                entity.OnUpdateLifecycle(this, Time.Current);

            UpdateProps();

            foreach (var entity in ObjectStore.Values.Where(x => x is RpgEntity).Cast<RpgEntity>())
                entity.OnStateUpdateLifecycle(this, Time.Current);

            UpdateProps();
        }
    }
}
