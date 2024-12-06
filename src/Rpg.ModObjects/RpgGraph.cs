using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects
{
    public class RpgGraph
    {
        private List<PropRef> UpdatedProps = new List<PropRef>();

        public RpgEntity Context { get; set; }
        public RpgEntity? Initiator { get; set; }
        private ObjectsDictionary ObjectStore { get; set; } = new();

        public Temporal Time { get; init; } = new Temporal();

        public RpgGraph(RpgEntity context, RpgEntity? initiator = null)
        {
            RpgTypeScan.RegisterAssembly(GetType().Assembly);

            Context = context;
            Initiator = initiator ?? context;

            Time.OnTemporalEvent += OnTemporalEvent;
            Time.Transition(PointInTimeType.Waiting);
        }

        public RpgGraph(RpgGraphState state)
        {
            RpgTypeScan.RegisterAssembly(GetType().Assembly);

            Context = (RpgEntity)state.Entities.First(x => x.Id == state.ContextId);
            Initiator = state.InitiatorId != null
                ? (RpgEntity)state.Entities.First(x => x.Id == state.InitiatorId)
                : null;

            var builder = new RpgGraphBuilder();
            var rpgObjs = builder.Build(state.Entities.ToArray());

            foreach (var rpgObj in rpgObjs)
                if (!ObjectStore.ContainsKey(rpgObj.Id))
                    ObjectStore.Add(rpgObj.Id, rpgObj);

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
                InitiatorId = Initiator?.Id,
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

        //public Mod[] GetActiveMods(RpgObject? rpgObj, string? prop, Func<Mod, bool>? filterFunc = null)
        //    => rpgObj?.GetActiveMods(prop)
        //        .Where(x => filterFunc == null || filterFunc(x))
        //        .ToArray() ?? Array.Empty<Mod>();

        ////public Mod[] GetActiveMods(PropRef? propRef, Func<Mod, bool>? filterFunc = null)
        ////    => GetActiveMods(GetObject(propRef?.EntityId), propRef?.Prop, filterFunc);

        //public Mod[] GetActiveMods(Func<Mod, bool>? filterFunc = null)
        //    => ObjectStore.Values
        //        .SelectMany(x => x.GetActiveMods())
        //        .Where(x => filterFunc == null || filterFunc(x))
        //        .ToArray();

        //public Mod[] GetMods(PropRef? propRef, Func<Mod, bool>? filterFunc = null)
        //    => GetObject(propRef?.EntityId)
        //        ?.GetMods(propRef?.Prop)
        //        .Where(x => filterFunc == null || filterFunc(x))
        //        .ToArray() ?? Array.Empty<Mod>();

        //public Mod[] GetMods(Func<Mod, bool>? filterFunc = null)
        //    => ObjectStore.Values
        //        .SelectMany(x => x.GetMods())
        //        .Where(x => filterFunc == null || filterFunc(x))
        //        .ToArray();

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

        public T? GetContext<T>()
            where T : RpgObject
                => Context as T;

        public T? GetInitiator<T>()
            where T : RpgObject
                => Initiator as T;

        public T? GetObject<T>(string? objectId)
            where T : RpgObject
                => objectId != null && ObjectStore.ContainsKey(objectId)
                    ? ObjectStore[objectId] as T
                    : null;

        public T[] GetObjectsOwnedBy<T>(string? ownerId)
            where T : RpgObject
                => ownerId != null
                    ? ObjectStore.Values
                        .Where(x => x.OwnerId == ownerId && x.Expiry == LifecycleExpiry.Active && x is T)
                        .Cast<T>()
                        .ToArray()
                    : [];

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

        //public Dice? CalculateInitialPropValue(RpgObject? entity, string prop)
        //    => CalculatePropValue(entity, prop, mod => mod.IsInitialMod());

        //public Dice? CalculateBasePropValue(RpgObject? entity, string prop)
        //    => CalculatePropValue(entity, prop, mod => mod.IsBaseMod());

        //public Dice? CalculateOriginalBasePropValue(PropRef propRef)
        //    => CalculatePropValue(propRef, mod => mod.IsOriginalBaseMod());

        //public Dice? CalculateOriginalBasePropValue(RpgObject? entity, string prop)
        //    => CalculatePropValue(entity, prop, mod => mod.IsOriginalBaseMod());

        //public Dice? CalculatePropValue(PropRef propRef, Func<Mod, bool>? filterFunc = null)
        //    => CalculatePropValue(propRef.EntityId, propRef.Prop, filterFunc);

        //public Dice? CalculatePropValue(string entityId, string prop, Func<Mod, bool>? filterFunc = null)
        //{
        //    var entity = GetObject(entityId);
        //    return CalculatePropValue(entity, prop, filterFunc);
        //}

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        //public Dice? CalculatePropValue(RpgObject? entity, string? prop, Func<Mod, bool>? filterFunc = null)
        //{
        //    if (entity == null || string.IsNullOrEmpty(prop))
        //        return null;

        //    var mods = filterFunc != null
        //        ? GetActiveMods(entity, prop, filterFunc)
        //        : GetActiveMods(entity, prop);

        //    var dice = CalculateModsValue(mods);
        //    return dice;
        //}

        //public Dice? CalculateModsValue(Mod[] mods)
        //{
        //    var selectedMods = mods.Where(x => !(x.Behavior is Threshold));
        //    if (!selectedMods.Any())
        //        return null;

        //    Dice? dice = null;
        //    foreach (var mod in selectedMods)
        //    {
        //        var val = mod.Value();
        //        if (val != null)
        //            dice = dice != null ? dice.Value + val.Value : val;
        //    }

        //    return dice != null ? ApplyThreshold(mods, dice!.Value) : null;
        //}


        //public Dice? CalculateModValue(Mod? mod)
        //{
        //    if (mod == null)
        //        return null;

        //    Dice? value = mod.SourceValue ?? GetPropValue(GetObject(mod.Source!.EntityId), mod.Source.Prop);

        //    if (value != null && mod.SourceValueFunc != null)
        //    {
        //        var args = new Dictionary<string, object?>();
        //        args.Add(mod.SourceValueFunc.Args.First().Name, value);

        //        var entity = GetObject(mod.SourceValueFunc.EntityId);
        //        value = entity != null
        //            ? mod.SourceValueFunc.Execute(entity, args)
        //            : mod.SourceValueFunc.Execute(args);
        //    }

        //    return value;
        //}

        //private Dice ApplyThreshold(Mod[] mods, Dice dice)
        //{
        //    var threshold = mods.FirstOrDefault(x => x.Behavior is Threshold)?.Behavior as Threshold;
        //    if (threshold != null && dice.IsConstant)
        //    {
        //        if (dice.Roll() < threshold.Min)
        //            dice = threshold.Min;
        //        else if (dice.Roll() > threshold.Max)
        //            dice = threshold.Max;
        //    }

        //    return dice;
        //}

        public void OnPropUpdated(PropRef propRef)
            => UpdatedProps.Merge(propRef);

        public void OnPropsUpdated()
        {
            foreach (var obj in ObjectStore.Values)
                foreach (var prop in obj.GetProps())
                    UpdatedProps.Merge(new PropRef(prop.EntityId, prop.Name));
        }

        private void UpdatePropValues()
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
            {
                var entity = GetObject(propRef.EntityId);
                if (entity != null)
                {
                    var oldValue = entity.Value(propRef.Prop);
                    var newValue = entity.Value(propRef.Prop, calculate: true);

                    if (oldValue == null || oldValue != newValue)
                        entity.SetValue(propRef.Prop, newValue);
                }
            }
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
            var builder = new RpgGraphBuilder();
            var rpgObjs = builder.Build(Context);

            foreach (var rpgObj in rpgObjs)
                AddObject(rpgObj);
        }

        private void OnTimeBegins()
        {
            foreach (var obj in ObjectStore.Values.Where(x => x.Expiry == LifecycleExpiry.Unset))
                obj.OnTimeBegins();

            foreach (var obj in ObjectStore.Values.Where(x => x.Expiry == LifecycleExpiry.Unset))
                obj.OnStartLifecycle();

            OnPropsUpdated();
            UpdatePropValues();

            OnPropsUpdated();
            UpdatePropValues();
        }

        private void OnTimeUpdates()
        {
            foreach (var entity in ObjectStore.Values)
            {
                entity.OnUpdateStateLifecycle();
                entity.OnUpdateLifecycle();
            }

            UpdatePropValues();

            foreach (var entity in ObjectStore.Values)
            {
                if (entity.OnUpdateStateLifecycle())
                    entity.OnUpdateLifecycle();
            }

            UpdatePropValues();
        }
    }
}
