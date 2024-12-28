using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Rpg.Experimental.Mods;
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
            ChangeTracker.OnPropUpdated(toObjectId, toProp);

            var objData = GetObjectData(objectId)!;
            objData.ParentId = toObjectId;
        }

        public void Add(string objectId, string toObjectId, string toProp, TimePoint now)
            => Add(objectId, toObjectId, toProp, now, TimePointType.TimeEnds);

        public RpgGraph Add(Mod mod)
        {
            var propData = GetObjectData(mod.Target.ObjectId)?.GetPropData<RpgPropertyDataModdable>(mod.Target.Prop);
            if (propData != null && !propData.Mods.Any(x => x.Id == mod.Id))
                mod.ModBehavior.OnAdding(mod, this, propData);

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
            var objects = new RpgObjectTraverser().Build(obj, (rpgObj, parentObj) =>
            {
                if (!Objects.ContainsKey(rpgObj.Id))
                {
                    Objects.Add(rpgObj.Id, rpgObj);
                    ObjectData.Add(rpgObj.Id, CreateObject(rpgObj, parentObj));
                }
            });
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
            ChangeTracker.OnPropUpdated(mod.Target);
        }

        private void OnTemporalEvent(object? sender, TemporalEventArgs e)
        {
            foreach (var obj in Objects.Values)
                obj.OnTimeEvent(this);

            foreach (var objData in ObjectData.Values)
                objData.OnTimeEvent(this);

            foreach (var byObjId in ChangeTracker.UpdatedProps.GroupBy(x => x.ObjectId))
            {
                var objData = GetObjectData(byObjId.Key);
                foreach (var propRef in byObjId)
                    objData?.GetPropData(propRef.Prop)?.OnSyncProperty(this);
            }

            ChangeTracker.UpdatedProps.Clear();
        }

        public RpgObject? GetObject(string? objectId)
            => objectId != null && Objects.ContainsKey(objectId)
                ? Objects[objectId] as RpgObject
                : null;

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

            if (propertyInfo.PropertyOfType(typeof(int)))
                return new RpgPropertyDataModdable(objectId, propertyInfo.Name, RpgPropertyType.Int, propertyInfo.PropertyType.PropertyOfNullableType(typeof(int)));

            if (propertyInfo.PropertyOfType(typeof(Dice)))
                return new RpgPropertyDataModdable(objectId, propertyInfo.Name, RpgPropertyType.Dice, propertyInfo.PropertyType.PropertyOfNullableType(typeof(Dice)));

            if (propertyInfo.PropertyOfType(typeof(RpgObject)))
                return new RpgPropertyDataObject(objectId, propertyInfo.Name, RpgPropertyType.Child);

            if (propertyInfo.PropertyOfType(typeof(ICollection<RpgObject>)))
                return new RpgPropertyDataObject(objectId, propertyInfo.Name, RpgPropertyType.Children);

            return null;
        }
    }
}
