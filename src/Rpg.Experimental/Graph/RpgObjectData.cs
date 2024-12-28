using Newtonsoft.Json;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public class RpgObjectData : ILifecycle
    {
        public string? ParentId { get; set; }
        public string ObjectId { get; set; }

        [JsonProperty] public List<IRpgPropertyData> Props { get; private set; } = new();

        [JsonConstructor] private RpgObjectData() { }

        public RpgObjectData(string objectId, string? parentId, IEnumerable<IRpgPropertyData> props)
        {
            ParentId = parentId;
            ObjectId = objectId;
            Props.AddRange(props);
        }

        public void Expire(RpgGraph graph)
            => Expire(graph, graph.Time.Now);

        public void Expire(RpgGraph graph, TimePoint expiryTime)
        {
            foreach (var objData in graph.ObjectData.Values)
                objData.ExpireRefsTo(graph, expiryTime, ObjectId);
        }

        public void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId)
        {
            if (ParentId == objectId)
                ParentId = null;

            foreach (var prop in Props)
                prop.ExpireRefsTo(graph, expiryTime, objectId);
        }

        public void OnCreating(RpgGraph graph, RpgObject obj)
        {
            foreach (var prop in Props)
                prop.OnCreating(graph, obj);
        }

        public void OnTimeEvent(RpgGraph graph)
        {
            foreach (var prop in Props)
                prop.OnTimeEvent(graph);
        }


        //public void Add(string objectId, string prop, TimePoint start, TimePoint end)
        //{
        //    var propData = GetPropData<RpgPropertyDataObject>(prop);
        //    propData?.AddRefTo(objectId, start, end);
        //}

        //public void Add(string objectId, string prop, TimePoint now)
        //{
        //    var propData = GetPropData<RpgPropertyDataObject>(prop);
        //    propData?.AddRefTo(objectId, now, TimePointType.TimeEnds);
        //}

        //public RpgObjectData Add(Mod mod)
        //{
        //    var propData = GetPropData<RpgPropertyDataModdable>(mod.Target.Prop);
        //    if (propData != null)
        //        propData.Mods.Add(mod);

        //    return this;
        //}

        //public RpgObjectData Add<TEntity, TTargetValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TEntity : RpgObject
        //{
        //    var mod = new Mod(ModType.Base).Set(entity, targetExpr, dice);
        //    return Add(mod);
        //}

        //public RpgObjectData Add<TEntity, TTargetValue, TSourceValue>(TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TEntity : RpgObject
        //{
        //    var mod = new Mod(ModType.Base).Set(entity, targetExpr, entity, sourceExpr);
        //    return Add(mod);
        //}

        //public RpgObjectData Add<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
        //    where TTarget : RpgObject
        //    where TSource : RpgObject
        //{
        //    var mod = new Mod(ModType.Base).Set(target, targetExpr, source, sourceExpr);
        //    return Add(mod);
        //}

        public IRpgPropertyData? GetPropData(string prop)
            => Props.FirstOrDefault(x => x.Prop == prop);

        public T? GetPropData<T>(string prop)
            where T : class, IRpgPropertyData
        {
            var propData = Props.FirstOrDefault(x => x.Prop == prop);
            return propData as T;
        }
    }
}
