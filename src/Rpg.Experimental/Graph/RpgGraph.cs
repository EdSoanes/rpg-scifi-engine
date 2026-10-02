using Newtonsoft.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.System;
using Rpg.Experimental.Time;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Temporal = Rpg.Experimental.Time.Temporal;

namespace Rpg.Experimental.Graph
{
    public abstract class RpgGraph
    {
        private RpgSystem _rpgSystem;

        [JsonProperty] public RpgObject Context { get; private set; }
        [JsonProperty] internal Dictionary<string, RpgObjectData> ObjectData { get; private set; } = new();
        [JsonProperty] internal Dictionary<string, RpgLifecycleObject> Objects { get; private set; } = new();
        [JsonProperty] public Temporal Time { get; private set; } = new();
        [JsonProperty] internal RpgGraphChangeTracker ChangeTracker { get; private set; } = new();
        internal RpgPropertyRefFactory PropertyRefs { get; private set; }

        /// <summary>
        /// Rolls the dice when the app is asked to. It can be replaced, e.g. to fix the results in a test.
        /// </summary>
        [JsonIgnore] public IRpgDiceRoller DiceRoller { get; set; } = new RpgRandomDiceRoller();

        /// <summary>
        /// Who rolls when a step of an action needs a roll and nobody has said
        /// </summary>
        [JsonProperty] public RpgRollMode RollMode { get; set; } = RpgRollMode.Ask;

        public RpgGraph(RpgObject context, RpgSystem? rpgSystem = null)
        {
            CreateNonTraversibleTypes();

            _rpgSystem = rpgSystem ?? RpgSystemFactory.Build();

            PropertyRefs = new RpgPropertyRefFactory(this);
            Context = context;
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
            RestoreState(graphState);
        }

        /// <summary>
        /// Replace everything in the graph with a saved state
        /// </summary>
        protected void RestoreState(RpgGraphState graphState)
        {
            Context = (RpgObject)graphState.Objects.First(x => x.Id == graphState.ContextId);

            Objects.Clear();
            foreach (var obj in graphState.Objects)
                Objects.Add(obj.Id, obj);
            
            ObjectData.Clear();
            foreach (var objData in graphState.ObjectData)
                ObjectData.Add(objData.ObjectId, objData);

            Time.OnTemporalEvent -= OnTemporalEvent;
            Time = graphState.Time;
            Time.OnTemporalEvent += OnTemporalEvent;

            ChangeTracker = new RpgGraphChangeTracker();
            _turnTrackingRequested = false;
            RollMode = graphState.RollMode;

            foreach (var objData in ObjectData.Values)
                objData.OnRestoring(this);

            ChangeTracker.AllPropsUpdated(this);

            //Mod sets and states first, then objects, then activity actions (which need their actions and activities)
            var restoreOrder = Objects.Values
                .OrderBy(x => x is RpgActivityAction ? 2 : x is RpgObject ? 1 : 0)
                .ToArray();

            foreach (var obj in restoreOrder)
                obj.OnRestoring(this);

            Time.Refresh();
        }

        public RpgSystem GetSystem()
            => _rpgSystem;

        public RpgPropertyRef[] GetChangedProperties()
            => ChangeTracker.UpdatedProps.ToArray();

        #region Time

        /// <summary>
        /// Start counting turns at the given turn number. Does nothing if turns are already being counted.
        /// </summary>
        public void BeginTurnTracking(int turn = 1)
            => Time.BeginTurnTracking(turn);

        public void NextTurn()
            => Time.NextTurn();

        /// <summary>
        /// Move to a turn. Time passes: moving forward several turns passes through each turn in order.
        /// </summary>
        public void AdvanceToTurn(int turn)
            => Time.ToTurn(turn);

        /// <summary>
        /// Stop counting turns. Anything that still had turns left to run is dropped. States that need turn
        /// tracking stay on. Lasting changes carry on.
        /// </summary>
        public void EndTurnTracking()
        {
            if (Time.IsTurnTracking)
                Time.EndEncounter();
        }

        /// <summary>
        /// A time event, either the built in "TimePasses" or one defined by the game system (e.g. "Sunrise").
        /// Any event can be triggered at any time, including during turn tracking, which it does not end.
        /// </summary>
        public void TriggerTimeEvent(string eventName)
            => Time.RaiseEvent(eventName);

        /// <summary>
        /// Change the number of the current turn. No time passes: everything measured in turns shifts by
        /// the same amount. Use this to agree a turn number with other character sheets.
        /// </summary>
        public virtual void RenumberTurn(int turn)
        {
            if (!Time.IsTurnTracking)
                return;

            var offset = turn - Time.Turn;
            if (offset == 0)
                return;

            foreach (var lifecycle in GetAllLifecycles())
                lifecycle.Item1.ShiftTurns(offset);

            Time.RenumberTurn(turn);
            Time.Refresh();
        }

        /// <summary>
        /// What currently needs turns to be counted
        /// </summary>
        public RpgTurnTrackingReport GetTurnTrackingReport()
            => new RpgTurnTrackingReport
            {
                IsTurnTracking = Time.IsTurnTracking,
                Turn = Time.Turn,
                Effects = GetAppliedTurnBasedMods()
                    .Where(x => x.StartsTurnTracking)
                    .ToArray(),
                States = Objects.Values
                    .OfType<RpgState>()
                    .Where(x => x.IsOn && x.NeedsTurnTracking)
                    .ToArray()
            };

        internal void RequestTurnTracking()
            => _turnTrackingRequested = true;

        /// <summary>
        /// Called when the state at the start of the current turn has changed in a way that should be kept
        /// if the turn is gone back to
        /// </summary>
        internal virtual void OnTurnStateChanged() { }

        protected virtual void OnAfterTemporalEvent(TemporalEventArgs e) { }

        private Mod[] GetAppliedTurnBasedMods()
            => GetAllLifecycles()
                .Select(x => x.Item1)
                .OfType<Mod>()
                .Where(x => x.IsTurnBased
                    && x.IsApplied
                    && x.IsUserEnabled != false
                    && x.Expired == null
                    && (x.Expiry == LifecycleExpiry.Active || x.Expiry == LifecycleExpiry.Pending))
                .ToArray();

        /// <summary>
        /// Every object, mod set, state, mod and child reference in the graph. Mods and child references
        /// come with the property they belong to.
        /// </summary>
        private List<(RpgLifecycleObject, RpgPropertyRef?)> GetAllLifecycles()
        {
            var res = new List<(RpgLifecycleObject, RpgPropertyRef?)>();

            foreach (var obj in Objects.Values)
                res.Add((obj, null));

            foreach (var objData in ObjectData.Values)
                foreach (var propData in objData.Props)
                {
                    var propRef = new RpgPropertyRef(propData.ObjectId, propData.Prop);
                    if (propData is RpgPropertyDataModdable moddable)
                        res.AddRange(moddable.Mods.Select(x => ((RpgLifecycleObject)x, (RpgPropertyRef?)propRef)));
                    else if (propData is RpgPropertyDataObject objectProp)
                        res.AddRange(objectProp.Refs.Select(x => ((RpgLifecycleObject)x, (RpgPropertyRef?)propRef)));
                }

            return res;
        }

        /// <summary>
        /// A time event has happened. Lifespans waiting for the event start, and lifespans lasting until the
        /// event end. During turn tracking this includes everything lasting "until time passes", because a
        /// time event does not end turn tracking.
        /// </summary>
        private void OnNamedTimeEvent(string eventName)
        {
            var now = Time.Now;
            var isTurnTracking = Time.IsTurnTracking;

            foreach (var (lifecycle, propRef) in GetAllLifecycles())
            {
                var changed = false;

                if (lifecycle.Start.IsEvent(eventName))
                {
                    lifecycle.StartAt(now);
                    changed = true;
                }
                else if (lifecycle.Expired == null && lifecycle.Start.Type != TimePointType.Event)
                {
                    //Something still waiting for its own start event is not ended by an earlier event
                    var endsNow = lifecycle.End.IsEvent(eventName)
                        || (isTurnTracking && lifecycle.End.Type == TimePointType.TimePasses);

                    if (endsNow)
                    {
                        lifecycle.Expire(now);
                        changed = true;
                    }
                }

                if (changed && propRef != null)
                    ChangeTracker.PropsUpdated(propRef);
            }
        }

        /// <summary>
        /// Turn tracking is ending. Anything that still has turns left to run is dropped. Anything that
        /// began during turn tracking and lasts until a time event carries on.
        /// </summary>
        private void OnTurnTrackingEnding()
        {
            var now = Time.Now;

            foreach (var (lifecycle, propRef) in GetAllLifecycles())
            {
                var changed = false;

                if (lifecycle.End.Type == TimePointType.Turn)
                {
                    if (lifecycle.Expired == null)
                    {
                        lifecycle.Expire(now);
                        changed = true;
                    }
                }
                else if (lifecycle.Start.Type == TimePointType.Turn && lifecycle.End.Type == TimePointType.Event)
                {
                    lifecycle.StartAt(TimePointType.TimeBegins);
                    changed = true;
                }

                if (changed && propRef != null)
                    ChangeTracker.PropsUpdated(propRef);
            }
        }

        /// <summary>
        /// Outside turn tracking nothing measured in turns can run. An applied effect that lasts a number of
        /// turns starts turn tracking, as does a state that needs turns counted switching on. Effects marked
        /// as too trivial for that are dropped.
        /// </summary>
        private void ResolveTurnBasedOutsideTurnTracking()
        {
            var turnBasedMods = GetAppliedTurnBasedMods();

            if (_turnTrackingRequested || turnBasedMods.Any(x => x.StartsTurnTracking))
            {
                _turnTrackingRequested = false;
                Time.BeginTurnTracking();
                return;
            }

            foreach (var mod in turnBasedMods)
            {
                mod.Expire(this, Time.Now);
                ChangeTracker.PropsUpdated(mod.Target);
            }
        }

        #endregion Time

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

        public RpgGraph Add<TEntity>(Mod mod, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            mod
                .SetTarget(entity, targetProp)
                .SetSource(dice, valueCalc);
            return Add(mod);
        }

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

        public RpgGraph Add<TTarget, TSource, TSourceValue>(Mod mod, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            mod
                .SetTarget(target, targetProp)
                .SetSource(source, sourceExpr, valueCalc);

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
                    nameof(Int32) => new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Dice, isNullable, true),
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

        public RpgGraph CreateVirtualProperty(RpgObject obj, string prop)
        {
            CreateVirtualProperty(obj.Id, prop, nameof(Dice), false, null);
            return this;
        }

        public RpgGraph CreateVirtualProperty(RpgObject obj, string prop, Dice initialValue)
        {
            CreateVirtualProperty(obj.Id, prop, nameof(Dice), false, new Dice(initialValue));
            return this;
        }

        public RpgGraph CreateVirtualProperty(string objectId, RpgArg arg)
        {
            CreateVirtualProperty(objectId, arg.Name, arg.Type, arg.IsNullable, arg.Value);
            return this;
        }

        public RpgGraph CreateVirtualProperties(RpgObject obj, RpgArg[] args)
        {
            foreach (var arg in args.Where(x => x.Type == nameof(Int32) || x.Type == nameof(Dice)))
            {
                CreateVirtualProperty(obj.Id, arg);

                //A whole number input needs its dice rolled before it has a value
                if (arg.Type == nameof(Int32))
                {
                    var propData = GetPropertyData<RpgPropertyDataModdable>(obj.Id, arg.Name);
                    if (propData != null)
                        propData.NeedsNumber = true;
                }
            }

            return this;
        }

        public RpgGraph SyncVirtualPropertyValues(RpgObject obj, (string, object?)[]? fromArgs)
        {
            if (fromArgs == null)
                return this;

            foreach (var propData in GetPropertyData<RpgPropertyDataModdable>(obj.Id).Where(x => x.IsVirtual))
            {
                if (fromArgs.Any(x => x.Item1 == propData.Prop))
                {
                    var arg = fromArgs.First(x => x.Item1 == propData.Prop);

                    //A number supplied for a value that has dice in it is the result of rolling those dice.
                    //It is stored as the roll and the bonuses stay as they are.
                    var expression = propData.GetExpression(this);
                    if (expression != null && !expression.Value.IsConstant && TryGetSuppliedNumber(arg.Item2, out var result))
                    {
                        propData.SetRoll(this, RpgRollSource.Player, result);
                        continue;
                    }

                    //Otherwise the mods are expired and the new value is added as a mod
                    propData.ResetToBase(this);
                    SetVirtualPropertyValue(obj, arg.Item1, arg.Item2);
                }
            }

            return this;
        }

        /// <summary>
        /// Set values on virtual properties that do not have a value yet. Existing values are left alone.
        /// </summary>
        public RpgGraph FillVirtualPropertyValues(RpgObject obj, (string, object?)[]? fromArgs)
        {
            if (fromArgs == null)
                return this;

            foreach (var propData in GetPropertyData<RpgPropertyDataModdable>(obj.Id).Where(x => x.IsVirtual))
            {
                if (fromArgs.Any(x => x.Item1 == propData.Prop) && propData.GetValue<Dice?>(this) == null)
                {
                    var arg = fromArgs.First(x => x.Item1 == propData.Prop);
                    SetVirtualPropertyValue(obj, arg.Item1, arg.Item2);
                }
            }

            return this;
        }

        /// <summary>
        /// The virtual properties of an object that currently have a value (state activation properties excluded)
        /// </summary>
        public (string, object?)[] GetVirtualPropertyValues(RpgObject obj)
            => GetPropertyData<RpgPropertyDataModdable>(obj.Id)
                .Where(x => x.IsVirtual && !RpgState.IsStateProp(x.Prop))
                .Select(x => (x.Prop, (object?)x.GetValue<Dice?>(this)))
                .Where(x => x.Item2 != null)
                .ToArray();

        /// <summary>
        /// Set an explicit value on a virtual property. The value is added as an Override so it replaces any
        /// Initial/Base value (e.g. an unrolled dice expression) whilst still allowing Standard mods to stack on top.
        /// </summary>
        public RpgGraph SetVirtualPropertyValue(RpgObject obj, string prop, object? value)
        {
            if (value == null)
                return this;

            Dice dice;
            if (value is Dice d)
                dice = d;
            else if (value is int i)
                dice = new Dice(i);
            else if (!Dice.TryParse(value, out dice))
                throw new ArgumentException($"Value '{value}' for {obj.Id}.{prop} is not an integer or dice expression");

            return Add(new Override()
                .SetTarget(obj.Id, prop)
                .SetSource(dice));
        }

        public RpgGraph Reset(RpgObject obj, string arg)
        {
            var propertyData = GetPropertyData<RpgPropertyDataModdable>(obj.Id, arg);
            propertyData?.ResetToBase(this);
            return this;
        }

        public RpgGraph ResetVirtualProperties(RpgObject obj)
        {
            foreach (var propData in GetPropertyData<RpgPropertyDataModdable>(obj.Id).Where(x => x.IsVirtual))
                propData.ResetToBase(this);

            return this;

        }

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

        internal MetaObject? GetMetaObject(string? archetype)
            => _rpgSystem.GetMetaObject(archetype);

        private int _temporalEventDepth;
        private bool _turnTrackingRequested;

        private void OnTemporalEvent(object? sender, TemporalEventArgs e)
        {
            _temporalEventDepth++;
            try
            {
                if (e.Event != null)
                    OnNamedTimeEvent(e.Event);

                if (e.Time.Type == TimePointType.EncounterEnds)
                    OnTurnTrackingEnding();

                ProcessTemporalEvent();
            }
            finally
            {
                _temporalEventDepth--;
            }

            if (_temporalEventDepth == 0)
            {
                if (Time.IsTurnTracking)
                    _turnTrackingRequested = false;
                else if (Time.Now.Type == TimePointType.Waiting)
                    ResolveTurnBasedOutsideTurnTracking();

                OnAfterTemporalEvent(e);
            }
        }

        private void ProcessTemporalEvent()
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

            //Activities before their actions. The actions take their lifespan from the activity
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
            if (objData != null && ChangeTracker.UnsyncedProperties(this, objectId))
                ChangeTracker.SyncProperties(this, objectId);
        }

        public RpgLifecycleObject? GetLifecycleObject(string? objectId)
            => objectId != null && Objects.ContainsKey(objectId)
                ? Objects[objectId]
                : null;

        public int GetObjectCount()
            => Objects.Count;

        public bool ObjectExists(string? id)
            => id != null && Objects.ContainsKey(id);

        public RpgObject? GetObject(string? objectId)
            => GetLifecycleObject(objectId) as RpgObject;

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
                        res.AddRange(modPropData.Mods.Where(x => x.OwnerId == id));

            return res.ToArray();
        }

        /// <summary>
        /// Take a mod out of the graph altogether
        /// </summary>
        public void Remove(Mod mod)
        {
            foreach (var objData in ObjectData.Values)
                foreach (var propData in objData.Props.OfType<RpgPropertyDataModdable>())
                    if (propData.Mods.RemoveAll(x => x.Id == mod.Id) > 0)
                        ChangeTracker.PropUpdated(propData.ObjectId, propData.Prop);
        }

        public Mod[] GetMods(IEnumerable<string> modIds)
        {
            var ids = modIds.ToHashSet();
            if (ids.Count == 0)
                return [];

            var res = new List<Mod>();
            foreach (var objData in ObjectData.Values)
                foreach (var propData in objData.Props)
                    if (propData is RpgPropertyDataModdable modPropData)
                        res.AddRange(modPropData.Mods.Where(x => ids.Contains(x.Id)));

            return res.ToArray();
        }

        public int GetObjectDataCount()
            => ObjectData.Count;

        public bool ObjectDataExists(string? id)
            => id != null && ObjectData.ContainsKey(id);

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

        public T[] GetPropertyData<T>(string? objectId)
            where T : class, IRpgPropertyData
                => GetObjectData(objectId)?.Props.Where(x => x is T).Cast<T>().ToArray() ?? [];

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
            activity.CreateActivityAction(this, action);

            return activity;
        }

        /// <summary>
        /// Continue the owner's current activity with an action nominated by the outcome of a previous action
        /// </summary>
        public RpgActivity CreateActivity(string activityOwnerId, RpgActionRef actionRef)
            => CreateActivity(activityOwnerId, actionRef.ActionOwnerId, actionRef.ActionName);

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

        #region Dice

        private static bool TryGetSuppliedNumber(object? value, out int number)
        {
            number = 0;
            if (value == null || value is Dice)
                return false;

            return int.TryParse(value.ToString(), out number);
        }

        /// <summary>
        /// The stored roll of a property, if it has one
        /// </summary>
        public RpgRoll? GetRoll(string objectId, string prop)
            => GetPropertyData<RpgPropertyDataModdable>(objectId, prop)?.Roll;

        /// <summary>
        /// The app rolls the dice in a property's value and the result is stored. It replaces any earlier
        /// roll. Returns null if the value has no dice in it.
        /// </summary>
        public RpgRoll? Roll(string objectId, string prop)
        {
            var roll = GetPropertyData<RpgPropertyDataModdable>(objectId, prop)?.SetRoll(this, RpgRollSource.App);
            if (roll != null)
                Time.Refresh();

            return roll;
        }

        public RpgRoll? Roll(RpgObject obj, string prop)
            => Roll(obj.Id, prop);

        /// <summary>
        /// Store the result of dice the player rolled for a property's value. The result is the total of
        /// the dice, without bonuses. It replaces any earlier roll. A result that is not possible on those
        /// dice is accepted and flagged. Returns null if the value has no dice in it.
        /// </summary>
        public RpgRoll? SetRoll(string objectId, string prop, int result)
        {
            var roll = GetPropertyData<RpgPropertyDataModdable>(objectId, prop)?.SetRoll(this, RpgRollSource.Player, result);
            if (roll != null)
                Time.Refresh();

            return roll;
        }

        public RpgRoll? SetRoll(RpgObject obj, string prop, int result)
            => SetRoll(obj.Id, prop, result);

        /// <summary>
        /// Remove the stored roll of a property, so its dice are unrolled again
        /// </summary>
        public bool ClearRoll(string objectId, string prop)
        {
            var cleared = GetPropertyData<RpgPropertyDataModdable>(objectId, prop)?.ClearRoll(this) ?? false;
            if (cleared)
                Time.Refresh();

            return cleared;
        }

        public bool ClearRoll(RpgObject obj, string prop)
            => ClearRoll(obj.Id, prop);

        /// <summary>
        /// Every roll that is needed and has not been settled
        /// </summary>
        public RpgPendingRoll[] GetPendingRolls()
            => ObjectData.Keys
                .SelectMany(GetPendingRolls)
                .ToArray();

        /// <summary>
        /// The rolls an object needs that have not been settled
        /// </summary>
        public RpgPendingRoll[] GetPendingRolls(string objectId)
        {
            var obj = GetObject(objectId);
            if (obj == null || obj.Expiry != LifecycleExpiry.Active)
                return [];

            var activityAction = obj as RpgActivityAction;
            if (activityAction != null && activityAction.IsComplete)
                return [];

            var res = new List<RpgPendingRoll>();
            foreach (var propData in GetPropertyData<RpgPropertyDataModdable>(objectId).Where(x => x.IsRollPending(this)))
            {
                string[] steps = [];
                if (activityAction != null)
                {
                    //Only the steps still to be done need the roll
                    steps = activityAction.StepsNeedingNumber(propData.Prop);
                    if (steps.Length == 0)
                        continue;
                }

                var expression = propData.GetExpression(this)!.Value;
                res.Add(new RpgPendingRoll
                {
                    ObjectId = objectId,
                    Prop = propData.Prop,
                    Expression = expression,
                    DicePart = expression.DicePart,
                    ActionName = activityAction?.GetAction()?.Name,
                    Steps = steps,
                    OutOfDateRoll = propData.Roll
                });
            }

            return res.ToArray();
        }

        /// <summary>
        /// The app rolls every pending roll
        /// </summary>
        public RpgRoll[] RollPending()
        {
            var rolls = GetPendingRolls()
                .Select(x => GetPropertyData<RpgPropertyDataModdable>(x.ObjectId, x.Prop)?.SetRoll(this, RpgRollSource.App))
                .Where(x => x != null)
                .Cast<RpgRoll>()
                .ToArray();

            if (rolls.Any())
                Time.Refresh();

            return rolls;
        }

        /// <summary>
        /// Rules code asks for dice to be rolled, e.g. in reaction to a turn or a time event. Nothing is
        /// rolled: the roll becomes pending until the app or the player settles it. The result is then the
        /// value of the property. Requesting the same dice again for the same property changes nothing, so
        /// rules code can ask every time it is called. Requesting different dice replaces the roll.
        /// </summary>
        public RpgGraph RequestRoll(RpgObject obj, string prop, Dice dice)
        {
            var propData = GetPropertyData<RpgPropertyDataModdable>(obj.Id, prop);
            if (propData == null)
            {
                CreateVirtualProperty(obj.Id, prop, nameof(Dice), false, dice);
                propData = GetPropertyData<RpgPropertyDataModdable>(obj.Id, prop);
            }
            else if (propData.NeedsNumber && propData.GetExpression(this) == dice)
            {
                return this;
            }
            else
            {
                propData.ResetToBase(this);
                propData.ClearRoll(this);
                Add(new Override()
                    .SetTarget(obj.Id, prop)
                    .SetSource(dice));
            }

            if (propData != null)
            {
                propData.NeedsNumber = true;
                ChangeTracker.PropUpdated(obj.Id, prop);
            }

            return this;
        }

        #endregion Dice

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
