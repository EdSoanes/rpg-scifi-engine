using Newtonsoft.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.System;
using Rpg.Experimental.Time;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.Experimental.Graph
{
    public class RpgGraph
    {
        private RpgSystem _rpgSystem;

        [JsonProperty] public RpgObject Context { get; private set; }
        [JsonProperty] public RpgObject Actor { get; private set; }
        [JsonProperty] public Dictionary<string, RpgObjectData> ObjectData { get; private set; } = new();
        [JsonProperty] public Dictionary<string, RpgLifecycleObject> Objects { get; private set; } = new();
        [JsonProperty] public Temporal Time { get; private set; } = new();
        [JsonProperty] public RpgGraphChangeTracker ChangeTracker { get; private set; } = new();
        public RpgPropertyRefFactory PropertyRefs { get; private set; }

        public RpgGraph(RpgObject context, RpgSystem? metaGraph = null)
            : this(context, context, metaGraph)
        { }

        public RpgGraph(RpgObject context, RpgObject actor, RpgSystem? rpgSystem = null)
        {
            CreateNonTraversibleTypes();

            _rpgSystem = rpgSystem ?? RpgSystemFactory.Build();

            PropertyRefs = new RpgPropertyRefFactory(this);
            Context = context;
            Actor = actor;
            Objects.Clear();
            ObjectData.Clear();
            Time.OnTemporalEvent += OnTemporalEvent;

            Add(Context);
            Time.BeginTime();
        }

        public RpgGraph(RpgGraphState graphState, RpgSystem rpgSystem)
        {
            CreateNonTraversibleTypes();

            _rpgSystem = rpgSystem;

            PropertyRefs = new RpgPropertyRefFactory(this);
            Context = (RpgObject)graphState.Objects.First(x => x.Id == graphState.ContextId);
            Actor = (RpgObject)graphState.Objects.First(x => x.Id == graphState.InitiatorId);

            Objects.Clear();
            foreach (var obj in graphState.Objects)
                Objects.Add(obj.Id, obj);
            
            ObjectData.Clear();
            foreach (var objData in graphState.ObjectData)
                ObjectData.Add(objData.ObjectId, objData);

            Time = graphState.Time;
            Time.OnTemporalEvent += OnTemporalEvent;

            Add(Context);
            Time.BeginTime();
        }

        public RpgSystem GetSystem()
            => _rpgSystem;

        public RpgGraphState GetGraphState()
        {
            var graphState = new RpgGraphState
            {
                Objects = Objects.Values.ToList(),
                ObjectData = ObjectData.Values.ToList(),
                ContextId = Context.Id,
                InitiatorId = Actor.Id,
                Time = Time
            };

            return graphState;
        }

        public void AddTo(string parentId, string parentProp, string childId, TimePoint start, TimePoint end)
        {
            var propData = GetObjectData(parentId)?.GetPropData<RpgPropertyDataObject>(parentProp);
            propData?.AddRefTo(childId, start, end);
            ChangeTracker.PropUpdated(parentId, parentProp);

            var objData = GetObjectData(childId)!;
            objData.ParentId = parentId;
        }

        public void AddTo(string parentId, string parentProp, string childId, TimePoint now)
            => AddTo(parentId, parentProp, childId, now, TimePointType.TimeEnds);

        public void Move(string parentId, string toProp, string childId)
        {
            Expire(childId);
            AddTo(parentId, toProp, childId, Time.Now);
        }

        public RpgGraph Add(Mod mod)
        {
            var targetRef = PropertyRefs.Create(mod.Target.ObjectId, mod.Target.Path);
            if (targetRef != null)
            {
                mod.SetTarget(targetRef);
                var propData = GetObjectData(targetRef.ObjectId)?.GetPropData<RpgPropertyDataModdable>(targetRef.Path);
                if (propData != null && !propData.Mods.Any(x => x.Id == mod.Id))
                {
                    mod.OnCreating(this, null);
                    propData.Mods.Add(mod);
                    ChangeTracker.PropUpdated(targetRef.ObjectId, targetRef.Path);
                }
            }

            return this;
        }

        public RpgGraph Add<TEntity, TTargetValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
                => Add(new Standard(), entity, targetExpr, dice, valueCalc);

        public RpgGraph Add<TEntity, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
                => Add(new Standard(), entity, targetExpr, sourceExpr, valueCalc);

        public RpgGraph Add<TEntity, TTargetValue>(Mod mod, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            mod
                .SetTarget(entity, targetExpr)
                .SetSource(dice, valueCalc);
            return Add(mod);
        }

        public RpgGraph Add<TEntity, TTargetValue, TSourceValue>(Mod mod, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            mod
                .SetTarget(entity, targetExpr)
                .SetSource(entity, sourceExpr, valueCalc);

            return Add(mod);
        }

        public RpgGraph Add<TTarget, TTargetValue, TSource, TSourceValue>(Mod mod, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            mod
                .SetTarget(target, targetExpr)
                .SetSource(source, sourceExpr, valueCalc);

            return Add(mod);
        }

        public void Add(RpgObject rootObj)
        {
            var objects = GetDescendantObjects(rootObj);
            foreach (var pair in objects)
            {
                if (!Objects.ContainsKey(pair.Item1.Id))
                {
                    Objects.Add(pair.Item1.Id, pair.Item1);
                    if (pair.Item1 is RpgObject rpgObj)
                    {
                        var objectData = CreateObjectData(rpgObj, pair.Item2);
                        if (objectData != null)
                            ObjectData.Add(rpgObj.Id, objectData);
                    }
                }
            }

            foreach (var pair in objects)
                GetObjectData(pair.Item1.Id)?.OnCreating(this, pair.Item1 as RpgObject);

            foreach (var pair in objects)
                pair.Item1.OnCreating(this, pair.Item1 as RpgObject);
        }

        public IRpgPropertyData Add(IRpgPropertyData propertyData)
        {
            var obj = GetObject(propertyData.ObjectId);
            var objData = GetObjectData(propertyData.ObjectId);
            if (obj != null && objData != null && !objData.Props.Any(x => x.Prop == propertyData.Prop))
            {
                objData.Props.Add(propertyData);
                propertyData.OnCreating(this, obj);
            }

            return propertyData;
        }

        public void Add(RpgModSet modSet)
        {
            Objects.Add(modSet.Id, modSet);
            modSet.OnCreating(this, null);
        }

        public IRpgPropertyData? CreateVirtualProperty(string objectId, string prop, string propType, bool isNullable, object? value = null)
        {
            var propData = GetPropertyData(objectId, prop);
            if (propData == null)
            {
                propData = propType switch
                {
                    nameof(Int32) => new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Int, isNullable, true),
                    nameof(Dice) => new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Dice, isNullable, true),
                    _ => RpgTypeUtilities.IsOftype<RpgObject>(propType)
                        ? new RpgPropertyDataObject(objectId, prop, RpgPropertyType.Child, true)
                        : null
                };

                if (propData != null)
                {
                    propData.OnCreatingVirtual(this, value);
                    propData = Add(propData);
                }
            }

            return propData;
        }

        public IRpgPropertyData? CreateVirtualProperty(string objectId, RpgArg arg)
            => CreateVirtualProperty(objectId, arg.Name, arg.Type, arg.IsNullable, arg.Value);

        public void Expire(string objectId, TimePoint expiryTime)
        {
            var objData = GetObjectData(objectId)!;
            objData?.Expire(this, expiryTime);
        }

        public void Expire(string objectId)
            => Expire(objectId, Time.Now);

        public void Expire(Mod mod)
            => Expire(mod, Time.Now);

        public void Expire(Mod mod, TimePoint expiryTime)
        {
            mod.Expire(this, expiryTime);
            ChangeTracker.PropsUpdated(mod.Target);
        }

        public RpgProperty[] GetProperties(string objectId)
        {
            var res = GetObjectData(objectId)?.Props
                .Where(x => !x.IsVirtual)
                .Select(x => x.GetProperty(this))
                .ToArray() ?? [];

            return res;
        }

        internal MetaProperty[] GetMetaProperties(string objectId)
        {
            var obj = GetObject(objectId);
            return GetMetaProperties(obj);
        }

        internal MetaProperty[] GetMetaProperties(RpgObject? obj)
        {
            var metaObject = _rpgSystem?.Objects.FirstOrDefault(x => x.Archetypes.Contains(obj?.Archetype));
            return metaObject?.Properties.ToArray() ?? [];
        }
    
        internal MetaProperty? GetMetaProperty(string objectId, string prop)
        {
            var obj = GetObject(objectId);
            var metaObject = _rpgSystem?.Objects.FirstOrDefault(x => x.Archetypes.Contains(obj?.Archetype));
            var metaProperty = metaObject?.Properties.FirstOrDefault(x => x.Prop == prop);

            return metaProperty;
        }

        private void OnTemporalEvent(object? sender, TemporalEventArgs e)
        {
            var modSets = Objects.Values.Where(x => x is RpgModSet && !(x is RpgState));
            OnTemporalEvent(modSets);

            var objects = Objects.Values.Where(x => x is RpgObject && !(x is RpgAction) && !(x is RpgActivity) && !(x is RpgActivityAction));
            OnTemporalEvent(objects);

            ChangeTracker.SyncProperties(this);

            var states = Objects.Values.Where(x => x is RpgState);
            OnTemporalEvent(states);

            ChangeTracker.SyncProperties(this);

            var actions = Objects.Values.Where(x => x is RpgAction);
            OnTemporalEvent(actions);

            var activityActions = Objects.Values.Where(x => x is RpgActivityAction);
            OnTemporalEvent(activityActions);

            var activities = Objects.Values.Where(x => x is RpgActivity);
            OnTemporalEvent(activities);

            ChangeTracker.SyncProperties(this);

            var toDelete = Objects.Values.Where(x => x.Expiry == LifecycleExpiry.Destroyed).ToList();
            foreach (var obj in toDelete)
            {
                Objects.Remove(obj.Id);
                if (ObjectData.ContainsKey(obj.Id))
                    ObjectData.Remove(obj.Id);
            }
        }

        public void OnTemporalEvent(IEnumerable<RpgLifecycleObject> objects)
        {
            foreach (var obj in objects)
                obj.OnTimeEvent(this);

            foreach (var obj in objects)
            {
                var objData = GetObjectData(obj.Id);
                if (objData != null && ChangeTracker.AddTimeEventObject(objData.ObjectId))
                    objData.OnTimeEvent(this);
            }
        }

        public void OnTimeEvent(string objectId)
        {
            var objData = GetObjectData(objectId);
            if (objData != null && ChangeTracker.AddTimeEventObject(objectId))
                objData?.OnTimeEvent(this);
        }

        public void OnSyncProperties(string objectId)
        {
            var objData = GetObjectData(objectId);
            if (objData != null && ChangeTracker.UnsyncedProperties(objectId))
                ChangeTracker.SyncProperties(this, objectId);
        }

        public RpgLifecycleObject? GetLifespan(string? objectId)
            => objectId != null && Objects.ContainsKey(objectId)
                ? Objects[objectId]
                : null;

        public RpgObject? GetObject(string? objectId)
            => GetLifespan(objectId) as RpgObject;

        public T[] GetOwnerObjects<T>(string? objectId)
            where T : RpgLifecycleObject
                => Objects.Values
                    .Where(x => x.OwnerId == objectId && x is T)
                    .Cast<T>()
                    .ToArray();

        public RpgObject[] GetOwnerObjects(string? objectId)
            => Objects.Values
                .Where(x => x.OwnerId == objectId && x is RpgObject)
                .Cast<RpgObject>()
                .ToArray();

        public RpgModSet[] GetOwnerModSets(string? objectId)
            => Objects.Values
                .Where(x => x is RpgModSet && !(x is RpgState) && x.OwnerId == objectId)
                .Cast<RpgModSet>()
                .ToArray();

        public RpgState[] GetObjectStates(string? objectId)
            => GetOwnerObjects<RpgState>(objectId);

        public string? ActivateState(string ownerId, string stateName, int duration)
        {
            var owner = GetObject(ownerId);
            var activation = owner?.CreateStateActivation(stateName, duration, true);
            if (activation != null)
            {
                Add(activation);
                return activation.Id;
            }

            return null;
        }

        public void DeactivateState(string ownerId, string activationId)
        {
            var objData = GetObjectData(ownerId);
            if (objData != null)
            {
                foreach (var propData in objData.Props.Where(x => x is RpgPropertyDataModdable).Select(x => x as RpgPropertyDataModdable))
                {
                    var stateMod = propData?.Mods.FirstOrDefault(x => x.Id == activationId && x.Expiry == LifecycleExpiry.Active);
                    if (stateMod != null)
                    {
                        stateMod.Expire(TimePointType.BeforeTime);
                        return;
                    }
                }
            }
        }

        public RpgState? GetObjectState(string? objectId, string stateName)
        {
            var state = Objects.Values.FirstOrDefault(x => x is RpgState state && state.OwnerId == objectId && state.Name == stateName) as RpgState;
            return state;
        }

        public RpgAction[] GetObjectActions(string? objectId)
        {
            var actions = Objects.Values
                .Where(x => x is RpgAction action && action.OwnerId == objectId)
                .Cast<RpgAction>()
                .ToArray();

            return actions;
        }

        public RpgAction? GetObjectAction(string? objectId, string actionName)
        {
            var action = Objects.Values.FirstOrDefault(x => x is RpgAction action && action.OwnerId == objectId && action.Name == actionName) as RpgAction;
            return action;
        }

        public Mod[] GetOwnerMods(string? id)
        {
            var res = new List<Mod>();
            foreach (var objData in ObjectData.Values)
                foreach (var propData in objData.Props)
                    if (propData is RpgPropertyDataModdable modPropData)
                        modPropData.Mods.Where(x => x.OwnerId == id);

            return res.ToArray();
        }

        public RpgObjectData? GetObjectData(string? objectId)
            => objectId != null && ObjectData.ContainsKey(objectId)
                ? ObjectData[objectId]
                : null;

        public IRpgPropertyData? GetPropertyData(string? objectId, string prop)
            => GetObjectData(objectId)
                ?.Props.FirstOrDefault(x => x.Prop == prop);

        public T? GetPropertyData<T>(string? objectId, string prop)
            where T : class, IRpgPropertyData
            => GetObjectData(objectId)
                ?.Props.FirstOrDefault(x => x.Prop == prop) as T;

        public void SetPropertyValue<T>(RpgObject? obj, string path, T? value)
        {
            var (propObj, prop) = PropertyRefs.GetObjectForPath(obj, path);
            if (propObj != null && prop != null)
            {
                var propInfo = propObj.GetType().GetProperty(prop);
                var setMethod = propInfo?.GetSetMethod(true);
                if (propInfo != null && setMethod != null && RpgTypeUtilities.PropertyOfType(propInfo, typeof(T)))
                    setMethod.Invoke(propObj, [value]);
            }
        }

        public T? GetPropertyValue<T>(string objectId, string path)
        {
            var obj = GetObject(objectId);
            return GetPropertyValue<T>(obj, path);
        }

        public T? GetPropertyValue<T>(RpgObject? obj, string path)
        {
            var (propObj, prop) = PropertyRefs.GetObjectForPath(obj, path);
            if (propObj != null && prop != null)
            {
                var propInfo = propObj.GetType().GetProperty(prop);
                if (propInfo != null)
                {
                    var value = propInfo.GetValue(propObj);
                    if (value is T)
                        return (T)value;
                }
                else
                {
                    var propData = GetPropertyData(propObj.Id, prop);
                    if (propData != null)
                        return propData!.GetValue<T>(this);
                }
            }

            return default;
        }

        public RpgActivity CreateActivity(string activityOwnerId, string actionOwnerId, string actionName)
        {
            var action = GetOwnerObjects<RpgAction>(actionOwnerId).FirstOrDefault(x => x.Name == actionName);
            if (action == null)
                throw new ArgumentException($"Could not find action {actionOwnerId} {actionName}");

            var activity = CreateActivity(activityOwnerId);

            var activityAction = new RpgActivityAction(activity, action, activity.ActivityActions.Count() + 1);
            Add(activityAction);
            AddTo(activity.Id, nameof(RpgActivity.ActivityActions), activityAction.Id, activity.Start, activity.End);

            OnTemporalEvent([activityAction, activity]);
            ChangeTracker.SyncProperties(this, activityAction.Id);
            ChangeTracker.SyncProperties(this, activity.Id);

            return activity;
        }

        private RpgActivity CreateActivity(string activityOwnerId)
        {
            var owner = GetObject(activityOwnerId);
            if (owner == null) throw new ArgumentException($"Activity Owner {activityOwnerId} not found for activity");

            var activity = GetOwnerObjects<RpgActivity>(activityOwnerId)
                .FirstOrDefault(x => x.Expiry == LifecycleExpiry.Active);

            if (activity == null)
            {
                var end = Time.Now.IsEncounterTime
                    ? new TimePoint(TimePointType.Turn, Time.Now.Count + 1)
                    : new TimePoint(TimePointType.TimePasses);

                activity = new RpgActivity(owner, Time.Now, end);
                Add(activity);
            }

            return activity;
        }

        #region Create Object Data

        private Type[] _nonTraversibleTypes = [];

        private void CreateNonTraversibleTypes()
        {
            if (_nonTraversibleTypes == null)
            {
                var res = typeof(RpgSystem).Assembly.GetTypes()
                    .Where(x => x.IsClass
                        && !x.IsAssignableTo(typeof(RpgObject)))
                    .ToList();

                res.AddRange([
                    typeof(string),
                    typeof(DateTime),
                    typeof(Guid),
                    //typeof(Mod),
                    //typeof(ModSet),
                    //typeof(State),
                    //typeof(ActionTemplate)
                ]);

                res.Remove(typeof(RpgLifecycleObject));

                _nonTraversibleTypes = res.ToArray();
            }
        }

        private (RpgLifecycleObject, RpgLifecycleObject?)[] GetDescendantObjects(RpgObject root)
        {
            var objects = new List<(RpgLifecycleObject, RpgLifecycleObject?)>();

            TraverseObjects(objects, root, null);

            return objects.ToArray();
        }

        private void TraverseObjects(List<(RpgLifecycleObject, RpgLifecycleObject?)> objects, object obj, RpgObject? parentObj)
        {
            if (obj is RpgObject rpgObj)
            {
                if (objects.Any(x => x.Item1.Id == rpgObj.Id))
                    return;

                objects.Add((rpgObj, parentObj));

                var stateObjects = CreateStateObjects(rpgObj);
                foreach (var stateObject in stateObjects)
                    objects.Add((stateObject, rpgObj));

                var actionObjects = CreateActionObjects(rpgObj);
                foreach (var actionObject in actionObjects)
                    objects.Add((actionObject, rpgObj));
            }

            var propertyInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                var items = GetPropertyObjects(obj, propertyInfo, out var isEnumerable);
                foreach (var item in items.Where(x => IsTraversibleType(x.GetType())))
                {
                    TraverseObjects(objects, item, obj as RpgObject);
                }
            }
        }

        private bool IsTraversibleType(Type type)
        {
            if (!type.IsClass)
                return false;

            if (string.IsNullOrEmpty(type.Namespace))
                return false;

            if (type.Namespace.StartsWith("System.") && !type.IsAssignableTo(typeof(IEnumerable)))
                return false;

            if (_nonTraversibleTypes.Any(x => type.IsAssignableTo(x)))
                return false;

            return true;
        }

        private IEnumerable<object> GetPropertyObjects(object context, PropertyInfo propertyInfo, out bool isEnumerable)
        {
            isEnumerable = false;

            if (propertyInfo.GetMethod?.Name == "get_Item")
                return Enumerable.Empty<object>();

            var obj = propertyInfo.GetValue(context, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

            if (obj == null)
                return Enumerable.Empty<object>();

            var items = GetPropObjects(obj!, out isEnumerable);
            return items;
        }

        private List<object> GetPropObjects(object? obj, out bool isEnumerable)
        {
            isEnumerable = false;

            var res = new List<object>();
            var items = new List<object?>();
            if (obj is IDictionary)
            {
                items = (obj as IDictionary)!.Values.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj is IEnumerable)
            {
                items = (obj as IEnumerable)!.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj != null)
                res.Add(obj);

            foreach (var item in items.Where(x => x != null))
                res.AddRange(GetPropObjects(item, out var _));

            return res;
        }

        private RpgObjectData? CreateObjectData(RpgObject obj, RpgLifecycleObject? parentObj)
        {
            var metaObject = _rpgSystem.GetMetaObject(obj.Archetype);
            if (metaObject == null)
                return null;

            var propData = metaObject.Properties
                .Where(x => x.PropertyType != RpgPropertyType.Text)
                .Select(x => CreatePropertyData(obj.Id, x))
                .Where(x => x != null)
                .Cast<IRpgPropertyData>()
                .ToArray();

            var objectData = new RpgObjectData(obj.Id, parentObj?.Id, propData);
            return objectData;
        }

        private IRpgPropertyData? CreatePropertyData(string objId, MetaProperty metaProperty)
        {
            IRpgPropertyData? propertyData = metaProperty.PropertyType switch
            {
                RpgPropertyType.Int => new RpgPropertyDataModdable(objId, metaProperty),
                RpgPropertyType.Dice => new RpgPropertyDataModdable(objId, metaProperty),
                RpgPropertyType.Child => new RpgPropertyDataObject(objId, metaProperty),
                RpgPropertyType.Children => new RpgPropertyDataObject(objId, metaProperty),
                _ => null
            };

            return propertyData;
        }

        private RpgState[] CreateStateObjects(RpgObject owner)
        {
            var types = RpgTypeUtilities.ForTypes<RpgState>()
                .Where(x => IsOwnerStateType(owner, x));

            var states = new List<RpgState>();
            foreach (var type in types)
            {
                var state = (RpgState)Activator.CreateInstance(type, [owner])!;
                states.Add(state);
            }

            return states.ToArray();
        }

        private bool IsOwnerStateType(RpgObject obj, Type? stateType)
        {
            while (stateType != null)
            {
                if (stateType.IsGenericType)
                {
                    var genericTypes = stateType.GetGenericArguments();
                    if (genericTypes.Length == 1 && obj.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                stateType = stateType.BaseType;
            }

            return false;
        }

        private RpgAction[] CreateActionObjects(RpgObject obj)
        {
            var actions = new List<RpgAction>();

            var types = RpgTypeUtilities.ForSubTypes(typeof(RpgAction))
                .Where(x => IsOwnerActionType(obj, x));

            foreach (var type in types)
            {
                var action = (RpgAction)Activator.CreateInstance(type, [obj])!;
                if (obj.IsA(action.OwnerArchetype!))
                    actions.Add(action);
            }

            return actions.ToArray();
        }

        private bool IsOwnerActionType(RpgObject entity, Type? actionType)
        {
            while (actionType != null)
            {
                if (actionType.IsGenericType)
                {
                    var genericTypes = actionType.GetGenericArguments();
                    if (genericTypes.Length == 1 && entity.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                actionType = actionType.BaseType;
            }

            return false;
        }

        #endregion Create Object Data
    }
}
