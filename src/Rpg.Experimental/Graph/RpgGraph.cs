using Newtonsoft.Json;
using Rpg.Experimental.Activities;
using Rpg.Experimental.Mods;
using Rpg.Experimental.ModSets;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.States;
using Rpg.Experimental.Time;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Rpg.Experimental.Graph
{
    public class RpgGraph
    {
        [JsonProperty] public RpgObject Context { get; private set; }
        [JsonProperty] public RpgObject Actor { get; private set; }
        [JsonProperty] public Dictionary<string, RpgObjectData> ObjectData { get; private set; } = new();
        [JsonProperty] public Dictionary<string, Lifespan> Objects { get; private set; } = new();
        [JsonProperty] public Temporal Time { get; private set; } = new();
        [JsonProperty] public RpgGraphChangeTracker ChangeTracker { get; private set; } = new();
        public RpgPropertyRefCreator PropertyRefs { get; private set; }

        public RpgGraph(RpgObject context)
            : this(context, context)
        { }

        public RpgGraph(RpgObject context, RpgObject actor)
        {
            PropertyRefs = new RpgPropertyRefCreator(this);
            Context = context;
            Actor = actor;
            Objects.Clear();
            ObjectData.Clear();
            Time.OnTemporalEvent += OnTemporalEvent;

            Add(Context);
            Time.BeginTime();
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
        {
            var mod = new Standard()
                .SetTarget(entity, targetExpr)
                .SetSource(dice);
            return Add(mod);
        }

        public RpgGraph Add<TEntity, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = new Standard()
                .SetTarget(entity, targetExpr)
                .SetSource(entity, sourceExpr);

            return Add(mod);
        }

        public RpgGraph Add<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = new Standard()
                .SetTarget(target, targetExpr)
                .SetSource(source, sourceExpr);

            return Add(mod);
        }

        public void Add(RpgObject rootObj)
        {
            var propertyCreator = new RpgPropertyDataCreator();
            var stateCreator = new RpgStateCreator();
            var actionCreator = new RpgActionCreator();
            var objects = new List<Lifespan>();

            Action<Lifespan, Lifespan?> OnAdding = (obj, parentObj) =>
            {
                if (!Objects.ContainsKey(obj.Id))
                {
                    objects.Add(obj);
                    Objects.Add(obj.Id, obj);
                    if (obj is RpgObject rpgObj)
                    {
                        var props = propertyCreator.CreatePropertyData(rpgObj);
                        var objectData = new RpgObjectData(rpgObj.Id, parentObj?.Id, props);
                        ObjectData.Add(rpgObj.Id, objectData);
                    }
                }
            };

            new RpgObjectCreator().Build(rootObj, (rpgObj, parentObj) =>
            {
                OnAdding(rpgObj, parentObj);
                var states = stateCreator.CreateStates(rpgObj);
                foreach (var state in states)
                    OnAdding(state, rpgObj);

                var actions = actionCreator.CreateActions(rpgObj);
                foreach (var action in actions)
                    OnAdding(action, rpgObj);
            });

            foreach (var obj in objects)
                GetObjectData(obj.Id)?.OnCreating(this, obj as RpgObject);

            foreach (var obj in objects)
                obj.OnCreating(this, obj as RpgObject);
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

        public void Add(ModSet modSet)
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
                    nameof(Int32) => new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Int, isNullable),
                    nameof(Dice) => new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Dice, isNullable),
                    _ => RpgTypeUtilities.IsOftype<RpgObject>(propType)
                        ? new RpgPropertyDataObject(objectId, prop, RpgPropertyType.Child)
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

        private void OnTemporalEvent(object? sender, TemporalEventArgs e)
        {
            var modSets = Objects.Values.Where(x => x is ModSet && !(x is State));
            OnTemporalEvent(modSets);

            var objects = Objects.Values.Where(x => x is RpgObject && !(x is RpgAction) && !(x is RpgActivity) && !(x is RpgActivityAction));
            OnTemporalEvent(objects);

            ChangeTracker.SyncProperties(this);

            var states = Objects.Values.Where(x => x is State);
            OnTemporalEvent(states);

            ChangeTracker.SyncProperties(this);

            var actions = Objects.Values.Where(x => x is RpgAction);
            OnTemporalEvent(actions);

            var activities = Objects.Values.Where(x => x is RpgActivity);
            OnTemporalEvent(activities);

            var activityActions = Objects.Values.Where(x => x is RpgActivityAction);
            OnTemporalEvent(activityActions);

            ChangeTracker.SyncProperties(this);

            var toDelete = Objects.Values.Where(x => x.Expiry == LifecycleExpiry.Destroyed).ToList();
            foreach (var obj in toDelete)
            {
                Objects.Remove(obj.Id);
                if (ObjectData.ContainsKey(obj.Id))
                    ObjectData.Remove(obj.Id);
            }
        }

        public void OnTemporalEvent(IEnumerable<Lifespan> objects)
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

        public Lifespan? GetLifespan(string? objectId)
            => objectId != null && Objects.ContainsKey(objectId)
                ? Objects[objectId]
                : null;

        public RpgObject? GetObject(string? objectId)
            => GetLifespan(objectId) as RpgObject;

        public T[] GetOwnerObjects<T>(string? objectId)
            where T : Lifespan
                => Objects.Values
                    .Where(x => x.OwnerId == objectId && x is T)
                    .Cast<T>()
                    .ToArray();

        public RpgObject[] GetOwnerObjects(string? objectId)
            => Objects.Values
                .Where(x => x.OwnerId == objectId && x is RpgObject)
                .Cast<RpgObject>()
                .ToArray();

        public ModSet[] GetOwnerModSets(string? objectId)
            => Objects.Values
                .Where(x => x is ModSet && !(x is State) && x.OwnerId == objectId)
                .Cast<ModSet>()
                .ToArray();

        public State[] GetObjectStates(string? objectId)
            => GetOwnerObjects<State>(objectId);

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

        public State? GetObjectState(string? objectId, string stateName)
        {
            var state = Objects.Values.FirstOrDefault(x => x is State state && state.OwnerId == objectId && state.Name == stateName) as State;
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

                //Virtual property...?
                else
                {
                    var propData = GetPropertyData(propObj.Id, prop);
                    if (propData != null)
                        return propData!.GetValue<T>(this);
                }
            }

            return default;
        }

        public RpgActivity GetObjectActivity(string ownerId, string? actionOwnerId = null, string? actionName = null)
        {
            var owner = GetObject(ownerId);
            if (owner == null) throw new ArgumentException("Owner not found for activity");

            var activity = GetOwnerObjects<RpgActivity>(ownerId)
                .FirstOrDefault(x => x.Expiry == LifecycleExpiry.Active);

            if (activity == null)
            {
                var end = Time.Now.IsEncounterTime
                    ? new TimePoint(TimePointType.Turn, Time.Now.Count + 1)
                    : new TimePoint(TimePointType.TimePasses);

                activity = new RpgActivity(owner, Time.Now, end);
                Add(activity);
            }

            return actionOwnerId != null && actionName != null
                ? CreateActivityAction(ownerId, actionOwnerId, actionName)
                : activity;
        }

        public RpgActivity CreateActivityAction(string ownerId, string actionOwnerId, string actionName)
        {
            var activity = GetOwnerObjects<RpgActivity>(ownerId)
                .FirstOrDefault(x => x.Expiry == LifecycleExpiry.Active);

            if (activity == null)
                throw new ArgumentException("Could not find activity");

            var action = GetOwnerObjects<RpgAction>(actionOwnerId).FirstOrDefault(x => x.Name == actionName);
            if (action == null)
                throw new ArgumentException("Could not find action");

            var activityAction = new RpgActivityAction(activity, action, activity.ActivityActions.Count() + 1);
            Add(activityAction);
            AddTo(activity.Id, nameof(RpgActivity.ActivityActions), activityAction.Id, activity.Start, activity.End);
            
            OnTemporalEvent([activityAction, activity]);
            ChangeTracker.SyncProperties(this, activityAction.Id);
            ChangeTracker.SyncProperties(this, activity.Id);

            return activity;
        }
    }
}
