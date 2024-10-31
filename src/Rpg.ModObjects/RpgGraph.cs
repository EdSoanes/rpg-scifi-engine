using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class RpgGraph
    {
        private List<PropRef> UpdatedProps = new List<PropRef>();

        [JsonProperty] public RpgEntity Context { get; private set; }
        [JsonProperty] protected ObjectsDictionary ObjectStore { get; set; } = new();

        [JsonProperty] public Temporal Time { get; init; } = new Temporal();

        public RpgGraph(RpgEntity context)
        {
            RpgTypeScan.RegisterAssembly(GetType().Assembly);

            Context = context;

            Time.OnTemporalEvent += OnTemporalEvent;
            Time.Transition(PointInTimeType.Waiting);
        }

        public RpgGraph(RpgGraphState state)
        {
            RpgTypeScan.RegisterAssembly(GetType().Assembly);

            Context = (RpgEntity)state.Entities.First(x => x.Id == state.ContextId);

            foreach (var entity in state.Entities.SelectMany(x => x.Traverse()))
                if (!ObjectStore.ContainsKey(entity.Id))
                    ObjectStore.Add(entity.Id, entity);

            foreach (var entity in ObjectStore.Values)
                entity.OnRestoring(this, entity);

            Time = state.Time!;
            Time.OnTemporalEvent += OnTemporalEvent;
            OnPropsUpdated();
            Time.TriggerEvent();
        }

        public RpgGraphState GetGraphState()
        {
            var state = new RpgGraphState
            {
                ContextId = Context.Id,
                Entities = ObjectStore.Values.Where(x => x is RpgEntity || x is Activity).ToList(),
                Time = Time,
            };

            return state;
        }

        public ILifecycle? GetLifecycleObject(string? id)
        {
            var res = GetObject(id) as ILifecycle
                ?? GetState(id) as ILifecycle
                ?? GetModSet(id) as ILifecycle
                ?? GetObjects()
                    .SelectMany(x => x.GetMods())
                    .FirstOrDefault(x => x.Id == id);

            return res;
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

        public Mod[] GetMods(Func<Mod, bool>? filterFunc = null)
            => ObjectStore.Values
                .SelectMany(x => x.GetMods())
                .Where(x => filterFunc == null || filterFunc(x))
                .ToArray();

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
                    .Select(x => new PropRef(entity.Id, x.Name))
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

        public bool AddObject(RpgObject entity)
        {
            if (!ObjectStore.ContainsKey(entity.Id))
            {
                ObjectStore.Add(entity.Id, entity);
                entity.OnCreating(this);
                if (Time.Now.Type != PointInTimeType.BeforeTime)
                {
                    entity.OnTimeBegins();
                    entity.OnStartLifecycle();
                }

                return true;
            }

            return false;
        }
        public Activity CreateActivity<T>(T initiator, ActivityTemplate actionGroup)
            where T : RpgEntity
        {
            var activityNo = GetObjects<Activity>()
                .Where(x => x.InitiatorId == initiator.Id && x.Time == Time.Now)
                .Count();

            var activity = new Activity(initiator, activityNo);
            AddObject(activity);

            activity.Init(actionGroup);

            return activity;
        }

        public Activity CreateActivity<T>(T initiator, RpgEntity owner, string actionName)
            where T : RpgEntity
        {
            var activityNo = GetObjects<Activity>()
                .Where(x => x.InitiatorId == initiator.Id && x.Time == Time.Now)
                .Count();

            var activity = new Activity(initiator, activityNo);
            AddObject(activity);

            activity.Init(owner, actionName);

            return activity;
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

        public T? GetObject<T>(string? objectId)
            where T : RpgObject
                => objectId != null && ObjectStore.ContainsKey(objectId)
                    ? ObjectStore[objectId] as T
                    : null;

        public RpgObject? GetParentObject(RpgObject childObject)
            => GetParentObject(childObject.Id);

        public RpgObject? GetParentObject(string childObjectId)
        {
            foreach (var obj in ObjectStore.Values.Where(x => x.Id != childObjectId))
            {
                if (obj.IsParentObjectTo(childObjectId))
                    return obj; 
            }

            return null;
        }
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

        public IEnumerable<RpgObject> GetObjectsByScope(string rpgObjId, ModScope scope)
        {
            var rpgObj = GetObject(rpgObjId)!;
            if (scope == ModScope.Standard)
                return rpgObj != null
                    ? [rpgObj]
                    : [];

            if (scope == ModScope.ParentEntity)
            {
                var parent = rpgObj.GetParentObject();
                return parent != null
                    ? [parent]
                    : [];
            }

            //Children...
            var children = rpgObj.GetChildObjects();
            return scope == ModScope.ChildObjects
                ? children
                : children.Where(x => x is RpgComponent).ToArray();
        }

        public ModSet[] GetModSets()
            => ObjectStore.Values
                .SelectMany(x => x.ModSets.Values)
                .ToArray();

        public T[] GetModSets<T>()
            where T : ModSet
                => GetModSets()
                    .Where(x => x is T)
                    .Cast<T>()
                    .ToArray();

        public ModSet[] GetModSets(string rpgObjId, Func<ModSet, bool> filterFunc)
            => GetModSets(GetObject(rpgObjId), filterFunc);

        public ModSet[] GetModSets(RpgObject? rpgObj, Func<ModSet, bool> filterFunc)
                => rpgObj?.ModSets.Values
                    .Where(x => filterFunc(x))
                    .ToArray() ?? [];

        public T[] GetModSets<T>(string rpgObjId, Func<T, bool> filterFunc)
            where T : ModSet
                => GetModSets(GetObject(rpgObjId), filterFunc);

        public T[] GetModSets<T>(RpgObject? rpgObj, Func<T, bool> filterFunc)
            where T : ModSet
                => rpgObj?.ModSets.Values
                    .Where(x => x is T)
                    .Cast<T>()
                    .Where(x => filterFunc(x))
                    .ToArray() ?? [];

        public ModSet? GetModSet(string? modSetId)
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
                var state = rpgObj.GetStateById(stateId);
                if (state != null)
                    return state;
            }

            return null;
        }

        public States.State? GetState(string? entityId, string stateName)
            => GetObject(entityId)?.GetState(stateName);

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

        public void RemoveModSet(string rpgObjId, string modSetId)
        {
            var rpgObj = GetObject(rpgObjId);
            var modSet = rpgObj?.GetModSet(modSetId);
            if (modSet != null)
                rpgObj!.RemoveModSet(modSet.Id);
        }

        public void RemoveModSet(string rpgObjId, ModSet modSet)
        {
            var rpgObj = GetObject(rpgObjId);
            if (rpgObj != null)
                rpgObj!.RemoveModSet(modSet.Id);
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

            Dice? value = mod.SourceValue ?? GetPropValue(GetObject(mod.Source!.EntityId), mod.Source.Prop);

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
                foreach (var prop in obj.GetProps())
                    UpdatedProps.Merge(new PropRef(prop.EntityId, prop.Name));
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
                entity.PropertyValue(prop, newValue ?? Dice.Zero);
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

        private void OnTemporalEvent(object? sender, TemporalEventArgs e)
        {
            switch (e.Time.Type)
            {
                case PointInTimeType.BeforeTime:
                    OnBeforeTime();
                    break;
                case PointInTimeType.TimeBegins:
                    OnTimeBegins();
                    break;
                default:
                    OnTimeUpdates();
                    break;
            }
        }

        private void OnBeforeTime()
        {
            foreach (var entity in Context.Traverse())
                AddObject(entity);
        }

        private void OnTimeBegins()
        {
            foreach (var obj in ObjectStore.Values.Where(x => x.Expiry == LifecycleExpiry.Unset))
                obj.OnTimeBegins();

            foreach (var obj in ObjectStore.Values.Where(x => x.Expiry == LifecycleExpiry.Unset))
                obj.OnStartLifecycle();

            OnPropsUpdated();
            UpdateProps();

            OnPropsUpdated();
            UpdateProps();
        }

        private void OnTimeUpdates()
        {
            foreach (var entity in ObjectStore.Values)
            {
                entity.OnUpdateStateLifecycle();
                entity.OnUpdateLifecycle();
            }

            UpdateProps();

            foreach (var entity in ObjectStore.Values)
            {
                if (entity.OnUpdateStateLifecycle())
                    entity.OnUpdateLifecycle();
            }

            UpdateProps();
        }
    }
}
