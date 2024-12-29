using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.ModSets;
using Rpg.Experimental.States;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public class RpgGraph
    {
        [JsonProperty] public RpgObject Context { get; private set; }
        [JsonProperty] public Dictionary<string, RpgObjectData> ObjectData { get; private set; } = new();
        [JsonProperty] public Dictionary<string, Lifespan> Objects { get; private set; } = new();
        [JsonProperty] public Temporal Time { get; private set; } = new();
        [JsonProperty] public RpgGraphChangeTracker ChangeTracker { get; private set; } = new();

        public RpgGraph(RpgObject context)
        {
            Context = context;
            Objects.Clear();
            ObjectData.Clear();
            Time.OnTemporalEvent += OnTemporalEvent;

            Add(Context);
            Time.BeginTime();
        }

        public void Add(string objectId, string toObjectId, string toProp, TimePoint start, TimePoint end)
        {
            var propData = GetObjectData(toObjectId)?.GetPropData<RpgPropertyDataObject>(toProp);
            propData?.AddRefTo(objectId, start, end);
            ChangeTracker.PropUpdated(toObjectId, toProp);

            var objData = GetObjectData(objectId)!;
            objData.ParentId = toObjectId;
        }

        public void Add(string objectId, string toObjectId, string toProp, TimePoint now)
            => Add(objectId, toObjectId, toProp, now, TimePointType.TimeEnds);

        public RpgGraph Add(Mod mod)
        {
            var propData = GetObjectData(mod.Target.ObjectId)?.GetPropData<RpgPropertyDataModdable>(mod.Target.Prop);
            if (propData != null && !propData.Mods.Any(x => x.Id == mod.Id))
            {
                mod.OnCreating(this, null);
                mod.ModBehavior.OnAdding(mod, this, propData);
            }

            return this;
        }

        public RpgGraph Add<TEntity, TTargetValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = new Mod(ModType.Base)
                .SetTarget(entity, targetExpr)
                .SetSource(dice);
            return Add(mod);
        }

        public RpgGraph Add<TEntity, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TEntity : RpgObject
        {
            var mod = new Mod(ModType.Base)
                .SetTarget(entity, targetExpr)
                .SetSource(entity, sourceExpr);

            return Add(mod);
        }

        public RpgGraph Add<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = new Mod(ModType.Base)
                .SetTarget(target, targetExpr)
                .SetSource(source, sourceExpr);

            return Add(mod);
        }

        public void Add(RpgObject obj)
        {
            var stateCreator = new RpgStateCreator();
            var objects = new RpgObjectCreator().Build(obj, (rpgObj, parentObj) =>
            {
                if (!Objects.ContainsKey(rpgObj.Id))
                {
                    Objects.Add(rpgObj.Id, rpgObj);

                    var props = CreateProperties(obj);
                    var objectData = new RpgObjectData(obj.Id, parentObj?.Id, props);
                    ObjectData.Add(rpgObj.Id, objectData);

                    objectData.OnCreating(this, obj);
                    obj.OnCreating(this, obj);
                }
            });

            foreach (var rpgObj in objects)
            {
                var states = stateCreator.CreateStates(this, rpgObj);
                foreach (var state in states)
                    Add(state);
            }
        }

        public void Add(ModSet modSet)
        {
            Objects.Add(modSet.Id, modSet);
            modSet.OnCreating(this, null);
        }

        public void Move(string objectId, string toObjectId, string toProp)
        {
            Expire(objectId);
            Add(objectId, toObjectId, toProp, Time.Now);
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

        private void OnTemporalEvent(object? sender, TemporalEventArgs e)
        {
            foreach (var obj in Objects.Values.Where(x => x is ModSet && !(x is State)))
                obj.OnTimeEvent(this);

            foreach (var obj in Objects.Values.Where(x => x is RpgObject))
                obj.OnTimeEvent(this);

            foreach (var objData in ObjectData.Values)
            {
                if (!ChangeTracker.IsObjectUpdated(objData.ObjectId))
                {
                    objData.OnTimeEvent(this);
                    ChangeTracker.ObjectUpdated(objData.ObjectId);
                }
            }

            ChangeTracker.Update(this);

            foreach (var obj in Objects.Values.Where(x => x is State))
                obj.OnTimeEvent(this);

            ChangeTracker.Update(this);

            var toDelete = Objects.Values.Where(x => x.Expiry == LifecycleExpiry.Destroyed).ToList();
            foreach (var obj in toDelete)
            {
                Objects.Remove(obj.Id);
                if (ObjectData.ContainsKey(obj.Id))
                    ObjectData.Remove(obj.Id);
            }
        }

        public Lifespan? RefreshObject(string objectId)
        {
            var obj = GetLifespan(objectId);
            if (obj != null)
                RefreshObject(obj);

            return obj;
        }

        private void RefreshObject(Lifespan obj)
        {
            if (obj != null && !ChangeTracker.IsObjectUpdated(obj.Id))
            {
                ChangeTracker.ObjectUpdated(obj.Id);

                obj.OnTimeEvent(this);
                var objData = GetObjectData(obj.Id);
                if (objData != null)
                    objData.OnTimeEvent(this);
            }
        }

        public Lifespan? GetLifespan(string? objectId)
            => objectId != null && Objects.ContainsKey(objectId)
                ? Objects[objectId]
                : null;

        public RpgObject? GetObject(string? objectId)
            => GetLifespan(objectId) as RpgObject;

        public State? GetObjectState(string? objectId, string stateName)
        {
            var state = Objects.Values.FirstOrDefault(x => x is State state && state.OwnerId == objectId && state.Name == stateName) as State;
            return state;
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

        private RpgObjectData CreateObject(RpgObject obj, RpgObject? parentObj)
        {
            var props = CreateProperties(obj);
            
            var objectData = new RpgObjectData(obj.Id, parentObj?.Id, props);
            objectData.OnCreating(this, obj);
            obj.OnCreating(this, obj);

            return objectData;
        }

        private IRpgPropertyData[] CreateProperties(RpgObject obj)
        {
            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(x => CreatePropertyData(obj.Id, x))
                .Where(x => x != null)
                .Cast<IRpgPropertyData>()
                .ToArray();
        }

        private IRpgPropertyData? CreatePropertyData(string objectId, PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic)
                return null;

            if (propertyInfo.SetMethod == null)
                return null;

            if (RpgTypeUtilities.PropertyOfType(propertyInfo, typeof(int)))
                return new RpgPropertyDataModdable(objectId, propertyInfo.Name, RpgPropertyType.Int, RpgTypeUtilities.PropertyOfNullableType(propertyInfo.PropertyType, typeof(int)));

            if (RpgTypeUtilities.PropertyOfType(propertyInfo, typeof(Dice)))
                return new RpgPropertyDataModdable(objectId, propertyInfo.Name, RpgPropertyType.Dice, RpgTypeUtilities.PropertyOfNullableType(propertyInfo.PropertyType, typeof(Dice)));

            if (RpgTypeUtilities.PropertyOfType(propertyInfo, typeof(RpgObject)))
                return new RpgPropertyDataObject(objectId, propertyInfo.Name, RpgPropertyType.Child);

            if (RpgTypeUtilities.PropertyOfType(propertyInfo, typeof(ICollection<RpgObject>)))
                return new RpgPropertyDataObject(objectId, propertyInfo.Name, RpgPropertyType.Children);

            return null;
        }
    }
}
