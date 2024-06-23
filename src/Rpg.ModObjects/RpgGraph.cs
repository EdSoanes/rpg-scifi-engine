using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public class RpgGraph
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        private List<PropRef> UpdatedProps = new List<PropRef>();

        [JsonProperty] public RpgObject Context { get; private set; }
        [JsonProperty] protected Dictionary<string, RpgObject> ObjectStore { get; set; } = new Dictionary<string, RpgObject>();
        [JsonProperty] public ITimeEngine Time { get; private set; }

        public RpgGraph(RpgObject context)
        {
            RpgReflection.RegisterAssembly(GetType().Assembly);

            Context = context;

            Time = new TurnBasedTimeEngine();
            Time.OnTimeEvent += OnTimeEvent;
            Time.Begin();
        }

        public T? Locate<T>(string? id)
            where T : class
        {
            if (typeof(T).IsAssignableTo(typeof(RpgObject)))
                return GetEntity(id) as T;

            if (typeof(T).IsAssignableTo(typeof(States.State)))
                return GetState(id) as T;

            if (typeof(T).IsAssignableTo(typeof(ModSet)))
                return GetModSet(id) as T;

            if (typeof(T).IsAssignableTo(typeof(Mod)))
                return GetEntities().SelectMany(x => x.PropStore.GetMods()).FirstOrDefault(x => x.Id == id) as T;

            return null;
        }

        public Mod[] GetMods(PropRef? propRef, bool filtered = true)
            => GetEntity(propRef?.EntityId)?.PropStore.GetMods(propRef!.Prop, filtered) ?? Array.Empty<Mod>();

        public Mod[] GetMods(PropRef? propRef, Func<Mod, bool> filterFunc)
            => GetEntity(propRef?.EntityId)?.PropStore.GetMods(propRef!.Prop, filterFunc) ?? Array.Empty<Mod>();

        public Mod[] GetMods(RpgObject? rpgObj, string prop, Func<Mod, bool> filterFunc)
            => rpgObj?.PropStore.GetMods(prop, filterFunc) ?? Array.Empty<Mod>();

        public Mod[] GetMods(RpgObject? rpgObj, string prop, bool filtered = true)
            => rpgObj?.PropStore.GetMods(prop, filtered) ?? Array.Empty<Mod>();

        public Mod[] GetMods(RpgObject? rpgObj, bool filtered = true)
            => rpgObj?.PropStore.GetMods(filtered).ToArray() 
                ?? Array.Empty<Mod>();

        public Mod[] GetMods(bool filtered = true)
            => ObjectStore.Values
                .SelectMany(x => GetMods(x, filtered)).ToArray() 
                ?? Array.Empty<Mod>();

        public void AddMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetEntity(modGroup.Key)?.PropStore.Add(modGroup.ToArray());
        }

        public void RemoveMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetEntity(modGroup.Key)?.PropStore.Remove(modGroup.ToArray());
        }

        public Prop? GetModProp(PropRef? propRef, bool create = false)
        {
            var entity = GetEntity(propRef?.EntityId);
            return entity?.PropStore.Get(propRef!.Prop, create);
        }

        public Prop[] GetModProps(RpgObject? rpgObj)
            => rpgObj?.PropStore.Get() ?? Array.Empty<Prop>();

        public List<PropRef> GetPropsAffectedBy(PropRef propRef)
        {
            var res = new List<PropRef>();
            res.Merge(propRef);

            var propsAffectedBy = new List<PropRef>();
            foreach (var entity in GetEntities())
            {
                var affectedBy = GetModProps(entity)
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

        public T? GetEntity<T>(string? entityId)
            where T : RpgObject
                => entityId != null && ObjectStore.ContainsKey(entityId)
                    ? ObjectStore[entityId] as T
                    : null;

        public RpgObject? GetEntity(string? entityId)
            => entityId != null && ObjectStore.ContainsKey(entityId)
                ? ObjectStore[entityId]
                : null;

        public IEnumerable<RpgObject> GetEntities()
            => ObjectStore.Values;

        public IEnumerable<T> GetEntities<T>()
            where T : RpgObject
                => ObjectStore.Values
                    .Where(x => x is T)
                    .Cast<T>();      

        public IEnumerable<RpgObject> GetScopedEntities(string rpgObjId, ModScope scope)
        {
            var entity = GetEntity(rpgObjId);
            if (entity is RpgComponent)
                entity = GetEntity((entity as RpgComponent)!.EntityId);

            var all = ObjectStore.Values.Where(x => x.Id == entity!.Id || (x as RpgComponent)?.EntityId == entity!.Id);
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
                .SelectMany(x => x.ModSetStore.Get())
                .ToArray();

        public ModSet? GetModSet(RpgObject rpgObj, string name)
        {
            var modSet = rpgObj.ModSetStore.Get(name);
            if (modSet != null)
                return modSet;

            return null;
        }

        public ModSet? GetModSet(string? modSetId)
        {
            if (modSetId == null) 
                return null;

            foreach (var rpgObj in ObjectStore.Values)
            {
                var modSet = rpgObj.ModSetStore[modSetId];
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
                    var state = entity.StateStore.Get().FirstOrDefault(x => x.Id == stateId);
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
                var modSet = rpgObj.ModSetStore[modSetId];
                if (modSet != null)
                {
                    rpgObj.ModSetStore.Remove(modSetId);
                    return;
                }
            }
        }

        public void RemoveModSet(RpgObject rpgObj, string modSetId)
        {
            var modSet = rpgObj.ModSetStore[modSetId];
            if (modSet != null)
                rpgObj.ModSetStore.Remove(modSet.Id);
        }

        public void RemoveModSetByName(RpgObject rpgObj, string name)
        {
            var modSet = rpgObj.ModSetStore.Get(name);
            if (modSet != null)
                rpgObj.ModSetStore.Remove(modSet.Id);
        }

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        public Dice? CalculatePropValue(PropRef propRef, Func<Mod, bool>? filterFunc = null)
        {
            var entity = GetEntity(propRef.EntityId);
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
                ? GetMods(entity, prop, filterFunc)
                : GetMods(entity, prop);

            var dice = CalculateModsValue(mods);
            return dice;
        }

        public Dice CalculateModsValue(Mod[] mods)
        {
            var dice = Dice.Zero;
            foreach (var mod in mods)
                dice += CalculateModValue(mod);

            return dice;
        }

        public Dice CalculateModValue(Mod? mod)
        {
            if (mod == null)
                return Dice.Zero;

            Dice value = mod.SourceValue ?? GetPropValue(GetEntity(mod.SourcePropRef!.EntityId), mod.SourcePropRef.Prop);

            if (mod.SourceValueFunc.IsCalc)
                value = mod.SourceValueFunc.Execute(this, value);

            return value;
        }

        public Dice GetPropValue(RpgObject? entity, string? prop)
        {
            if (entity == null || string.IsNullOrEmpty(prop))
                return Dice.Zero;

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
                var mods = GetMods(entity, prop);
                var dice = CalculateModsValue(mods);

                return dice;
            }

            return Dice.Zero;
        }

        public void OnPropUpdated(PropRef propRef)
            => UpdatedProps.Merge(propRef);

        public void OnPropsUpdated()
        {
            foreach (var obj in ObjectStore.Values)
                foreach (var modProp in GetModProps(obj))
                    UpdatedProps.Merge(modProp);
        }

        public void SetPropValue(PropRef propRef)
        {
            var entity = GetEntity(propRef.EntityId);
            SetPropValue(entity, propRef.Prop);
        }

        public void SetPropValue(RpgObject? entity, string prop)
        {
            var oldValue = GetPropValue(entity, prop);
            var newValue = CalculatePropValue(entity, prop);

            if (oldValue == null || oldValue != newValue)
                entity.PropertyValue(prop, newValue);
        }

        public Dice? GetInitialPropValue(RpgObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseInitMod);

        public Dice? GetBasePropValue(RpgObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseMod);

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
                AddEntity(entity);
                entity.OnBeforeTime(this, entity);
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
                entity.StateStore.OnUpdateLifecycle(this, Time.Current);

            UpdateProps();
        }
    }
}
